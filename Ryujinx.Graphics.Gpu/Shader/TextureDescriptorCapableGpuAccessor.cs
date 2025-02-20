﻿using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.Gpu.Shader
{
    abstract class TextureDescriptorCapableGpuAccessor : IGpuAccessor
    {
        private readonly GpuContext _context;

        public TextureDescriptorCapableGpuAccessor(GpuContext context)
        {
            _context = context;
        }

        public abstract ReadOnlySpan<ulong> GetCode(ulong address, int minimumSize);

        public abstract ITextureDescriptor GetTextureDescriptor(int handle, int cbufSlot);

        /// <summary>
        /// Queries host about the presence of the FrontFacing built-in variable bug.
        /// </summary>
        /// <returns>True if the bug is present on the host device used, false otherwise</returns>
        public bool QueryHostHasFrontFacingBug() => _context.Capabilities.HasFrontFacingBug;

        /// <summary>
        /// Queries host about the presence of the vector indexing bug.
        /// </summary>
        /// <returns>True if the bug is present on the host device used, false otherwise</returns>
        public bool QueryHostHasVectorIndexingBug() => _context.Capabilities.HasVectorIndexingBug;

        /// <summary>
        /// Queries host storage buffer alignment required.
        /// </summary>
        /// <returns>Host storage buffer alignment in bytes</returns>
        public int QueryHostStorageBufferOffsetAlignment() => _context.Capabilities.StorageBufferOffsetAlignment;

        /// <summary>
        /// Queries host support for readable images without a explicit format declaration on the shader.
        /// </summary>
        /// <returns>True if formatted image load is supported, false otherwise</returns>
        public bool QueryHostSupportsImageLoadFormatted() => _context.Capabilities.SupportsImageLoadFormatted;

        /// <summary>
        /// Queries host GPU non-constant texture offset support.
        /// </summary>
        /// <returns>True if the GPU and driver supports non-constant texture offsets, false otherwise</returns>
        public bool QueryHostSupportsNonConstantTextureOffset() => _context.Capabilities.SupportsNonConstantTextureOffset;

        /// <summary>
        /// Queries host GPU shader ballot support.
        /// </summary>
        /// <returns>True if the GPU and driver supports shader ballot, false otherwise</returns>
        public bool QueryHostSupportsShaderBallot() => _context.Capabilities.SupportsShaderBallot;

        /// <summary>
        /// Queries host GPU texture shadow LOD support.
        /// </summary>
        /// <returns>True if the GPU and driver supports texture shadow LOD, false otherwise</returns>
        public bool QueryHostSupportsTextureShadowLod() => _context.Capabilities.SupportsTextureShadowLod;

        /// <summary>
        /// Queries texture format information, for shaders using image load or store.
        /// </summary>
        /// <remarks>
        /// This only returns non-compressed color formats.
        /// If the format of the texture is a compressed, depth or unsupported format, then a default value is returned.
        /// </remarks>
        /// <param name="handle">Texture handle</param>
        /// <param name="cbufSlot">Constant buffer slot for the texture handle</param>
        /// <returns>Color format of the non-compressed texture</returns>
        public TextureFormat QueryTextureFormat(int handle, int cbufSlot = -1)
        {
            var descriptor = GetTextureDescriptor(handle, cbufSlot);

            if (!FormatTable.TryGetTextureFormat(descriptor.UnpackFormat(), descriptor.UnpackSrgb(), out FormatInfo formatInfo))
            {
                return TextureFormat.Unknown;
            }

            return formatInfo.Format switch
            {
                Format.R8Unorm           => TextureFormat.R8Unorm,
                Format.R8Snorm           => TextureFormat.R8Snorm,
                Format.R8Uint            => TextureFormat.R8Uint,
                Format.R8Sint            => TextureFormat.R8Sint,
                Format.R16Float          => TextureFormat.R16Float,
                Format.R16Unorm          => TextureFormat.R16Unorm,
                Format.R16Snorm          => TextureFormat.R16Snorm,
                Format.R16Uint           => TextureFormat.R16Uint,
                Format.R16Sint           => TextureFormat.R16Sint,
                Format.R32Float          => TextureFormat.R32Float,
                Format.R32Uint           => TextureFormat.R32Uint,
                Format.R32Sint           => TextureFormat.R32Sint,
                Format.R8G8Unorm         => TextureFormat.R8G8Unorm,
                Format.R8G8Snorm         => TextureFormat.R8G8Snorm,
                Format.R8G8Uint          => TextureFormat.R8G8Uint,
                Format.R8G8Sint          => TextureFormat.R8G8Sint,
                Format.R16G16Float       => TextureFormat.R16G16Float,
                Format.R16G16Unorm       => TextureFormat.R16G16Unorm,
                Format.R16G16Snorm       => TextureFormat.R16G16Snorm,
                Format.R16G16Uint        => TextureFormat.R16G16Uint,
                Format.R16G16Sint        => TextureFormat.R16G16Sint,
                Format.R32G32Float       => TextureFormat.R32G32Float,
                Format.R32G32Uint        => TextureFormat.R32G32Uint,
                Format.R32G32Sint        => TextureFormat.R32G32Sint,
                Format.R8G8B8A8Unorm     => TextureFormat.R8G8B8A8Unorm,
                Format.R8G8B8A8Snorm     => TextureFormat.R8G8B8A8Snorm,
                Format.R8G8B8A8Uint      => TextureFormat.R8G8B8A8Uint,
                Format.R8G8B8A8Sint      => TextureFormat.R8G8B8A8Sint,
                Format.R8G8B8A8Srgb      => TextureFormat.R8G8B8A8Unorm,
                Format.R16G16B16A16Float => TextureFormat.R16G16B16A16Float,
                Format.R16G16B16A16Unorm => TextureFormat.R16G16B16A16Unorm,
                Format.R16G16B16A16Snorm => TextureFormat.R16G16B16A16Snorm,
                Format.R16G16B16A16Uint  => TextureFormat.R16G16B16A16Uint,
                Format.R16G16B16A16Sint  => TextureFormat.R16G16B16A16Sint,
                Format.R32G32B32A32Float => TextureFormat.R32G32B32A32Float,
                Format.R32G32B32A32Uint  => TextureFormat.R32G32B32A32Uint,
                Format.R32G32B32A32Sint  => TextureFormat.R32G32B32A32Sint,
                Format.R10G10B10A2Unorm  => TextureFormat.R10G10B10A2Unorm,
                Format.R10G10B10A2Uint   => TextureFormat.R10G10B10A2Uint,
                Format.R11G11B10Float    => TextureFormat.R11G11B10Float,
                _                        => TextureFormat.Unknown
            };
        }

        /// <summary>
        /// Queries sampler type information.
        /// </summary>
        /// <param name="handle">Texture handle</param>
        /// <param name="cbufSlot">Constant buffer slot for the texture handle</param>
        /// <returns>The sampler type value for the given handle</returns>
        public SamplerType QuerySamplerType(int handle, int cbufSlot = -1)
        {
            return GetTextureDescriptor(handle, cbufSlot).UnpackTextureTarget().ConvertSamplerType();
        }

        /// <summary>
        /// Queries texture target information.
        /// </summary>
        /// <param name="handle">Texture handle</param>
        /// <param name="cbufSlot">Constant buffer slot for the texture handle</param>
        /// <returns>True if the texture is a rectangle texture, false otherwise</returns>
        public bool QueryIsTextureRectangle(int handle, int cbufSlot = -1)
        {
            var descriptor = GetTextureDescriptor(handle, cbufSlot);

            TextureTarget target = descriptor.UnpackTextureTarget();

            bool is2DTexture = target == TextureTarget.Texture2D ||
                               target == TextureTarget.Texture2DRect;

            return !descriptor.UnpackTextureCoordNormalized() && is2DTexture;
        }
    }
}
