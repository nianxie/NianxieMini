using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers.Binary;
using System.Linq;

namespace Nianxie.Riff
{
    // 使用 ArraySegment 存储数据
    // 而在解析过程中使用 Span 进行高性能运算
    public class RiffChunk
    {
        public readonly uint FourCC;
        public ArraySegment<byte> Data;// 依然保持对原始内存的引用，无拷贝

        public RiffChunk(uint fourCC, ArraySegment<byte> data)
        {
            FourCC = fourCC;
            Data = data;
        }

        public ReadOnlySpan<byte> AsSpan()
        {
            return new (Data.Array, Data.Offset, Data.Count);
        }

        public string GetUtf8String()
        {
            return Encoding.UTF8.GetString(Data.Array, Data.Offset, Data.Count);
        }
    }
    /// <summary>
    /// Riff文件，webp也是一种riff文件。
    /// </summary>
    public class RiffContainer
    {
        private static uint RIFF_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("RIFF"));
        private static uint WEBP_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("WEBP"));
        private static uint NX_CUSTOM_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("NX_C")); // nianxie custom fourCC id
        private static uint NX_MANIFEST_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("NX_M")); // nianxie custom fourCC id
        private static uint NX_BINARY_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("NX_B")); // nianxie custom fourCC id
        private IEnumerable<RiffChunk> AllChunks => WebpChunks.Concat(new []{CustomChunk, ManifestChunk}).Concat(BinaryChunks);
        private readonly RiffChunk[] WebpChunks;
        public readonly RiffChunk CustomChunk;
        public readonly RiffChunk ManifestChunk;
        public readonly List<RiffChunk> BinaryChunks;

        private RiffContainer(List<RiffChunk> chunks)
        {
            WebpChunks = chunks.Where(a => a.FourCC != NX_CUSTOM_UINT && a.FourCC != NX_MANIFEST_UINT && a.FourCC != NX_BINARY_UINT).ToArray();
            CustomChunk = chunks.FirstOrDefault(a => a.FourCC == NX_CUSTOM_UINT) ?? new RiffChunk(NX_CUSTOM_UINT, new byte[]{});
            ManifestChunk = chunks.FirstOrDefault(a => a.FourCC == NX_MANIFEST_UINT) ?? new RiffChunk(NX_MANIFEST_UINT, new byte[]{});
            BinaryChunks = chunks.Where(a => a.FourCC == NX_BINARY_UINT).ToList();
        }

        /// <summary>
        /// 序列化
        /// </summary>
        private byte[] Dump()
        {
            // 1. 计算总长度
            int totalSize = 12; // RIFF + Size + WEBP
            foreach (var chunk in AllChunks)
            {
                totalSize += 8 + chunk.Data.Count + (chunk.Data.Count % 2);
            }

            byte[] result = new byte[totalSize];
            Span<byte> dest = result;

            // 2. 写入 Header
            BinaryPrimitives.WriteUInt32LittleEndian(dest.Slice(0, 4), RIFF_UINT);
            BinaryPrimitives.WriteUInt32LittleEndian(dest.Slice(4, 4), (uint)totalSize - 8);
            BinaryPrimitives.WriteUInt32LittleEndian(dest.Slice(8, 4), WEBP_UINT);

            // 3. 写入 Chunks
            int offset = 12;
            foreach (var chunk in AllChunks)
            {
                // 写入 ID
                BinaryPrimitives.WriteUInt32LittleEndian(dest.Slice(offset, 4), chunk.FourCC);
                // 写入 Size
                BinaryPrimitives.WriteUInt32LittleEndian(dest.Slice(offset + 4, 4), (uint)chunk.Data.Count);
                
                offset += 8;

                // 写入内容 (Memory.Span.CopyTo 也是极速操作)
                chunk.AsSpan().CopyTo(dest.Slice(offset, chunk.Data.Count));
                offset += chunk.Data.Count;

                // 填充字节
                if (chunk.Data.Count % 2 != 0)
                {
                    dest[offset] = 0;
                    offset++;
                }
            }

            return result;
        }

        public static RiffContainer Load(byte[] source)
        {
            var chunks = new List<RiffChunk>();
            ReadOnlySpan<byte> span = source;

            // 1. 基础合法性检查
            if (span.Length < 12) throw new Exception("Data too short.");
            
            // 验证 RIFF 标志
            if (BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(0, 4)) != RIFF_UINT)
                throw new Exception("Not a RIFF file.");

            // 验证 WEBP 标志 (偏移 8)
            if (BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(8, 4)) != WEBP_UINT)
                throw new Exception("Not a WebP file.");

            // 2. 遍历 Chunks
            int offset = 12;
            while (offset + 8 <= span.Length)
            {
                // 读取 FourCC (4 bytes)
                uint fourCC = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset, 4));
                
                // 读取 Chunk Size (4 bytes, 小端序)
                uint chunkSize = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(offset + 4, 4));
                
                offset += 8; // 移动到 Data 起始位置

                if (offset + chunkSize > span.Length) throw new Exception("Chunk size out of bounds.");

                // 获取 Data 的切片 (使用 Memory 保证引用同一块内存而不拷贝)
                var chunkData = new ArraySegment<byte>(source, offset, (int)chunkSize);
                chunks.Add(new RiffChunk(fourCC, chunkData));

                // 移动偏移量
                offset += (int)chunkSize;

                // RIFF 对齐：如果是奇数，实际占用会多一个填充字节
                if (chunkSize % 2 != 0)
                {
                    offset++;
                }
            }

            return new RiffContainer(chunks);
        }

        public static byte[] Pack(byte[] webpData, string customStr, ManifestJson manifestJson, List<byte[]> binaries)
        {
            var riffContainer = Load(webpData);
            riffContainer.CustomChunk.Data = Encoding.UTF8.GetBytes(customStr);
            riffContainer.ManifestChunk.Data = Encoding.UTF8.GetBytes(manifestJson.Dump());
            riffContainer.BinaryChunks.Clear();
            foreach (var bin in binaries)
            {
                riffContainer.BinaryChunks.Add(new RiffChunk(NX_BINARY_UINT, bin));
            }
            return riffContainer.Dump();
        }

    }
}