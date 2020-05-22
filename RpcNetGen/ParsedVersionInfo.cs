namespace RpcNetGen
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ParsedVersionInfo
    {
        public ParsedVersionInfo(string versionId, string versionNumber, IEnumerable procedures)
        {
            VersionId = versionId;
            VersionNumber = versionNumber;
            Procedures = procedures?.Cast<ParsedProcedureInfo>().ToList() ?? new List<ParsedProcedureInfo>();
        }

        public string VersionNumber { get; }
        public string VersionId { get; }
        public List<ParsedProcedureInfo> Procedures { get; }
    }
}
