﻿using System.Numerics;
using SPICA.Formats.Common;
using SPICA.Math3D;
using SPICA.Serialization.Attributes;

namespace SPICA.Formats.CtrH3D.Model
{
    [Inline]
    public class H3DBone : INamed
    {
        private H3DBoneFlags _Flags;

        public Matrix3x4 InverseTransform;

        public H3DMetaData MetaData;

        [Padding (4)] public short ParentIndex;
        public Vector3 Rotation;

        public Vector3 Scale;
        public Vector3 Translation;

        public H3DBone ()
        {
            InverseTransform = new Matrix3x4 ();
        }

        public H3DBone (
            Vector3 Translation,
            Vector3 Rotation,
            Vector3 Scale,
            string Name,
            short Parent) : this ()
        {
            this.Translation = Translation;
            this.Rotation = Rotation;
            this.Scale = Scale;
            this.Name = Name;

            ParentIndex = Parent;
        }

        public H3DBoneFlags Flags {
            get => (H3DBoneFlags) BitUtils.MaskOutBits ((int) _Flags, 16, 3);
            set {
                var Value = BitUtils.MaskOutBits ((int) value, 16, 3);

                Value |= BitUtils.MaskBits ((int) _Flags, 16, 3);

                _Flags = (H3DBoneFlags) Value;
            }
        }

        public H3DBillboardMode BillboardMode {
            get => (H3DBillboardMode) BitUtils.GetBits ((int) _Flags, 16, 3);
            set => _Flags = (H3DBoneFlags) BitUtils.SetBits ((int) _Flags, (int) value, 16, 3);
        }

        public Matrix4x4 Transform {
            get {
                Matrix4x4 Transform;

                Transform = Matrix4x4.CreateScale (Scale);
                Transform *= Matrix4x4.CreateRotationX (Rotation.X);
                Transform *= Matrix4x4.CreateRotationY (Rotation.Y);
                Transform *= Matrix4x4.CreateRotationZ (Rotation.Z);
                Transform *= Matrix4x4.CreateTranslation (Translation);

                return Transform;
            }
        }

        public string Name { get; set; }

        public Matrix4x4 GetWorldTransform (H3DDict<H3DBone> Skeleton)
        {
            var Transform = Matrix4x4.Identity;

            var Bone = this;

            while (true) {
                Transform *= Bone.Transform;

                if (Bone.ParentIndex == -1) break;

                Bone = Skeleton[Bone.ParentIndex];
            }

            return Transform;
        }

        public void CalculateTransform (H3DDict<H3DBone> Skeleton)
        {
            var Transform = Matrix4x4.Identity;

            var Bone = this;

            while (true) {
                Transform *= Bone.Transform;

                if (Bone.ParentIndex == -1) break;

                Bone = Skeleton[Bone.ParentIndex];
            }

            var Mask =
                H3DBoneFlags.IsScaleUniform |
                H3DBoneFlags.IsScaleVolumeOne |
                H3DBoneFlags.IsRotationZero |
                H3DBoneFlags.IsTranslationZero;

            _Flags &= ~Mask;

            var ScaleUniform = Scale.X == Scale.Y && Scale.X == Scale.Z;

            if (ScaleUniform) _Flags = H3DBoneFlags.IsScaleUniform;
            if (Scale == Vector3.One) _Flags |= H3DBoneFlags.IsScaleVolumeOne;
            if (Rotation == Vector3.Zero) _Flags |= H3DBoneFlags.IsRotationZero;
            if (Translation == Vector3.Zero) _Flags |= H3DBoneFlags.IsTranslationZero;

            Matrix4x4 Inverse;
            Matrix4x4.Invert (Transform, out Inverse);

            InverseTransform = new Matrix3x4 (Inverse);
        }
    }
}