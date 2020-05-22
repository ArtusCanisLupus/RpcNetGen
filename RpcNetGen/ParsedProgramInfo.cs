namespace RpcNetGen
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ParsedProgramInfo
    {
        public ParsedProgramInfo(string programId, string programNumber, IEnumerable versions)
        {
            ProgramId = programId;
            ProgramNumber = programNumber;
            Versions = versions?.Cast<ParsedVersionInfo>().ToList() ?? new List<ParsedVersionInfo>();
        }

        public string ProgramNumber { get; }
        public string ProgramId { get; }
        public List<ParsedVersionInfo> Versions { get; }
    }
}
