﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TSQL.Elements;

namespace TSQL.Statements
{
	public abstract class TSQLStatement : TSQLElement
	{
		public abstract TSQLStatementType Type
		{
			get;
		}

		public TSQLSelectStatement AsSelect
		{
			get
			{
				return this as TSQLSelectStatement;
			}
		}

		public TSQLMergeStatement AsMerge
		{
			get
			{
				return this as TSQLMergeStatement;
			}
		}

		public TSQLUpdateStatement AsUpdate
		{
			get
			{
				return this as TSQLUpdateStatement;
			}
		}

		public TSQLDeleteStatement AsDelete
		{
			get
			{
				return this as TSQLDeleteStatement;
			}
		}

		public TSQLInsertStatement AsInsert
		{
			get
			{
				return this as TSQLInsertStatement;
			}
		}

		public TSQLExecuteStatement AsExecute
		{
			get
			{
				return this as TSQLExecuteStatement;
			}
		}
	}
}
