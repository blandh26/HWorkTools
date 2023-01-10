﻿using System;
using System.Linq;

namespace TSQL.Tokens
{
	public enum TSQLTokenType
	{
		/// <summary>
		///		Whitespace characters,
		///		i.e. line feeds or tabs.
		/// </summary>
		Whitespace,

		/// <summary>
		///		Special character in T-SQL,
		///		e.g. . or ,.
		/// </summary>
		Character,

		/// <summary>
		///		Object name, alias, or other reference,
		///		e.g. dbo.
		/// </summary>
		Identifier,

		/// <summary>
		///		Recognized T-SQL built-in reserved system identifier,
		///		e.g. OPENROWSET
		/// </summary>
		SystemIdentifier,

		/// <summary>
		///		Recognized T-SQL keyword,
		///		e.g. SELECT.
		/// </summary>
		Keyword,

		/// <summary>
		///		Comment starting with -- and continuing until the end of the line,
		///		e.g. -- this code creates a new lookup table.
		/// </summary>
		SingleLineComment,

		/// <summary>
		///		Comment spanning multiple lines starting with /* and ending with */
		///		e.g. /* here be dragons */.
		/// </summary>
		MultilineComment,

		/// <summary>
		///		Symbol representing an operation in T-SQL,
		///		e.g. + or !=.
		/// </summary>
		Operator,

		/// <summary>
		///		Variable starting with @,
		///		e.g. @id.
		/// </summary>
		Variable,

		/// <summary>
		///		Recognized server variables starting with @@,
		///		e.g. @@ROWCOUNT.
		/// </summary>
		SystemVariable,

		/// <summary>
		///		Simple numeric value, with or without a decimal, without sign,
		///		e.g. 210.5.
		/// </summary>
		NumericLiteral,

		/// <summary>
		///		Unicode or non-Unicode string value,
		///		e.g. 'Cincinnati' or N'München'.
		/// </summary>
		StringLiteral,

		/// <summary>
		///		Numeric value starting with a currency symbol,
		///		e.g. $4.25 or £3.42.
		/// </summary>
		MoneyLiteral,

		/// <summary>
		///		Binary value serialized as hexadecimal and starting with 0x,
		///		e.g. 0x69048AEFDD010E.
		/// </summary>
		BinaryLiteral,

		/// <summary>
		///		The beginning of a multi-line comment,
		///		e.g. /* something to comment.
		/// </summary>
		IncompleteComment,

		/// <summary>
		///		The beginning of a bracketed identifier,
		///		e.g. [dbo.
		/// </summary>
		IncompleteIdentifier,

		/// <summary>
		///		The beginning of a string literal,
		///		e.g. 'Cincinnat.
		/// </summary>
		IncompleteString,

		// https://docs.microsoft.com/en-us/dotnet/api/microsoft.sqlserver.transactsql.scriptdom.columntype
		// https://docs.microsoft.com/en-us/sql/t-sql/queries/select-clause-transact-sql
		// https://docs.microsoft.com/en-us/sql/t-sql/statements/merge-transact-sql

		/// <summary>
		///		An identifier specifying a system defined column,
		///		e.g. $IDENTITY, $ROWGUID, $ACTION.
		/// </summary>
		SystemColumnIdentifier
	}

	public static class TSQLTokenTypeExtensions
	{
		public static bool In(this TSQLTokenType type, params TSQLTokenType[] types)
		{
			return types.Any(t => type == t);
		}
	}
}
