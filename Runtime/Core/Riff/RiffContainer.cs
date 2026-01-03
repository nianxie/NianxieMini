using System;
using System.Collections.Generic;
using System.Text;
using System.Buffers.Binary;
using System.Linq;

namespace Nianxie.Riff
{
    /// <summary>
    /// Riff文件，webp也是一种riff文件。
    /// </summary>
    public class RiffContainer
    {
        // 使用 ReadOnlyMemory 存储数据，因为它比 Span 更适合存储在类字段中
        // 而在解析过程中使用 Span 进行高性能运算
        private class RiffChunk
        {
            public readonly uint FourCC;
            public ReadOnlyMemory<byte> Data;// 依然保持对原始内存的引用，无拷贝

            public RiffChunk(uint fourCC, ReadOnlyMemory<byte> data)
            {
                FourCC = fourCC;
                Data = data;
            }
        }
        private static uint RIFF_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("RIFF"));
        private static uint WEBP_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("WEBP"));
        private static uint NX_ARCHIVE_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("NX_A")); // nianxie custom fourCC id
        private static uint NX_MANIFEST_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("NX_M")); // nianxie custom fourCC id
        private static uint NX_BINARY_UINT = BinaryPrimitives.ReadUInt32LittleEndian(Encoding.ASCII.GetBytes("NX_B")); // nianxie custom fourCC id
        private IEnumerable<RiffChunk> AllChunks => WebpChunks.Concat(new []{ArchiveChunk, ManifestChunk}).Concat(BinaryChunks);
        private readonly RiffChunk[] WebpChunks;
        private readonly RiffChunk ArchiveChunk;
        private readonly RiffChunk ManifestChunk;
        private readonly List<RiffChunk> BinaryChunks;

        private RiffContainer(List<RiffChunk> chunks)
        {
            WebpChunks = chunks.Where(a => a.FourCC != NX_ARCHIVE_UINT && a.FourCC != NX_MANIFEST_UINT && a.FourCC != NX_BINARY_UINT).ToArray();
            ArchiveChunk = chunks.FirstOrDefault(a => a.FourCC == NX_ARCHIVE_UINT) ?? new RiffChunk(NX_ARCHIVE_UINT, new byte[]{});
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
                totalSize += 8 + chunk.Data.Length + (chunk.Data.Length % 2);
            }

            byte[] result = new byte[totalSize];
            Span<byte> dest = result.AsSpan();

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
                BinaryPrimitives.WriteUInt32LittleEndian(dest.Slice(offset + 4, 4), (uint)chunk.Data.Length);
                
                offset += 8;

                // 写入内容 (Memory.Span.CopyTo 也是极速操作)
                chunk.Data.Span.CopyTo(dest.Slice(offset, chunk.Data.Length));
                offset += chunk.Data.Length;

                // 填充字节
                if (chunk.Data.Length % 2 != 0)
                {
                    dest[offset] = 0;
                    offset++;
                }
            }

            return result;
        }

        private static RiffContainer LoadRiff(ReadOnlyMemory<byte> source)
        {
            var chunks = new List<RiffChunk>();
            ReadOnlySpan<byte> span = source.Span;

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
                var chunkData = source.Slice(offset, (int)chunkSize);
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

        public static byte[] Pack(byte[] webpData, ArchiveJson archiveJson, ManifestJson manifestJson, ReadOnlyMemory<byte>[] binaries)
        {
            return null;
        }

        public static void Unpack<TArchiveJson>(byte[] riffData, out TArchiveJson archiveJson, out ManifestJson manifestJson, out ReadOnlyMemory<byte>[] binaries) where TArchiveJson:ArchiveJson
        {
            //var riffContainer = LoadRiff(riffData);
            throw new NotImplementedException("TODO");
        }

    }
}