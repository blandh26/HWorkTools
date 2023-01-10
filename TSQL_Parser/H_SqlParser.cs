using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSQL.Clauses;
using TSQL.Clauses.Parsers;

namespace TSQL
{
    public static class H_SqlParser
    {
		public static TSQLSelectClause GetTSQLSelectClause(string sql)
		{
			using (StringReader reader = new StringReader(@"select a from b;"))
			using (TSQLTokenizer tokenizer = new TSQLTokenizer(reader))
			{
				tokenizer.MoveNext();

				TSQLSelectClause select = new TSQLSelectClauseParser().Parse(tokenizer);
				return select;
			}
		}

		public static TSQLFromClause GetTSQLFromClauseParser(string sql)
		{
			using (StringReader reader = new StringReader(@"select a from b;"))
			using (TSQLTokenizer tokenizer = new TSQLTokenizer(reader))
			{
				tokenizer.MoveNext();
				TSQLFromClause select1 = new TSQLFromClauseParser().Parse(tokenizer);
				return select1;
			}
		}
	}
}
