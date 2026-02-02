using System;
using UnityEngine;

namespace Nianxie.Craft
{
    public abstract class SourceKind
    {
    }

    public class ReleasedSourceKind:SourceKind
    {
        public static ReleasedSourceKind Instance = new();
        private ReleasedSourceKind()
        {
        }
    }

    public abstract class PackableSourceKind:SourceKind
    {
        public int packRiffIndex = -1;
    }

    public class UploadSourceKind:PackableSourceKind
    {
        public readonly Action<Texture2D> releaseUpload;
        public UploadSourceKind(Action<Texture2D> releaseUpload) 
        {
            this.releaseUpload=releaseUpload;
        }
    }
    public class RiffSourceKind : PackableSourceKind
    {
        public readonly int unpackRiffIndex;
        public RiffSourceKind(int riffIndex)
        {
            unpackRiffIndex = riffIndex;
        }
    }

    public class BuiltinSourceKind : SourceKind
    {
        public readonly string builtinPath;

        public BuiltinSourceKind(string builtinPath)
        {
            this.builtinPath = builtinPath;
        }
    }
}