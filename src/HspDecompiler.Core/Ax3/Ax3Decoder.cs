using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HspDecompiler.Core.Abstractions;
using HspDecompiler.Core.Ax3.Data;
using HspDecompiler.Core.Ax3.Data.Analyzer;
using HspDecompiler.Core.Ax3.Dictionary;
using HspDecompiler.Core.Exceptions;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.Ax3
{
    class Ax3Decoder : IAxDecoder
    {
        internal Ax3Decoder() { }

        private Hsp3Dictionary dictionary;

        internal Hsp3Dictionary Dictionary
        {
            get { return dictionary; }
            set { dictionary = value; }
        }

        public async Task<List<string>> DecodeAsync(BinaryReader reader, IDecompilerLogger logger, IProgressReporter progress, CancellationToken ct = default)
        {
            AxData data = new AxData();
            LexicalAnalyzer lex = null;
            TokenCollection stream = null;
            SyntacticAnalyzer synt = null;
            List<LogicalLine> lines = null;
            List<string> stringLines = new List<string>();
            try
            {
                logger.Write(Strings.AnalyzingHeader);
                data.LoadStart(reader, dictionary);
                data.ReadHeader();
                logger.Write(Strings.AnalyzingPreprocessor);
                data.ReadPreprocessor(dictionary);
                logger.Write(Strings.LexicalAnalysis);
                lex = new LexicalAnalyzer(dictionary);
                stream = lex.Analyze(data);
                data.LoadEnd();
                logger.Write(Strings.SyntacticAnalysis);
                synt = new SyntacticAnalyzer();
                lines = await synt.AnalyzeAsync(stream, data, logger, progress, ct);
                logger.Write(Strings.CreatingOutputFile);
                foreach (LogicalLine line in lines)
                {
                    if (line.Visible)
                    {
                        string str = new string('\t', line.TabCount);
                        stringLines.Add(str + line.ToString());
                    }
                }
            }
            catch (SystemException e)
            {
                throw new HspDecoderException("AxData", Strings.UnexpectedError, e);
            }
            return stringLines;
        }
    }
}
