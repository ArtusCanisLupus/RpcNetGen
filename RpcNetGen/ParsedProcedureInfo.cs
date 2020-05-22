namespace RpcNetGen
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ParsedProcedureInfo
    {
        public ParsedProcedureInfo(
            string procedureId,
            string procedureNumber,
            string resultType,
            IEnumerable parameters)
        {
            ProcedureId = procedureId;
            ProcedureNumber = procedureNumber;
            ResultType = resultType;
            Parameters = parameters?.Cast<ParsedDeclaration>().ToList() ?? new List<ParsedDeclaration>();
        }

        public string ProcedureNumber { get; }
        public string ProcedureId { get; set; }
        public string ResultType { get; }
        public List<ParsedDeclaration> Parameters { get; }
    }
}
