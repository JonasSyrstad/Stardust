// Guids.cs
// MUST match guids.h
using System;

namespace Stardust.Stardust_Tooling
{
    static class GuidList
    {
        public const string guidStardust_ToolingPkgString = "342809ef-3232-47eb-88ad-4426c1ac59e1";
        public const string guidStardust_ToolingCmdSetString = "69fea077-b6bb-4195-ab9b-d8623725dd25";

        public static readonly Guid guidStardust_ToolingCmdSet = new Guid(guidStardust_ToolingCmdSetString);
    };
}