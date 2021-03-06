/*
The MIT License (MIT)

Copyright (c) 2007 Roger Hill

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

using DAL.Standard.SqlMetadata;

namespace AutoCodeGenLibrary
{
    public abstract class CodeGeneratorBase
    {
        protected StringBuilder _WhitespaceWriter = new StringBuilder();

        public virtual string TabType
        {
            get { throw new NotImplementedException(); }
        }

        public CodeGeneratorBase() { }

        protected string GenerateAuthorNotice()
        {
            return $"Generated by Jolly Roger's Autocode Generator on {DateTime.Now}";
        }

        protected string AddTabs(int count, bool convertToSpaces = true)
        {
            int tabSize = Convert.ToInt32(ConfigurationManager.AppSettings[TabType]);
            return AddTabs(count, tabSize, convertToSpaces);
        }

        protected string AddTabs(int count, int tabSize, bool convertToSpaces)
        {
            if (count < 0)
                throw new ArgumentException($"Cannot generate a string of {count} tabs");

            if (tabSize < 1)
                throw new ArgumentException($"Tab size cannot be defined as less than zero white spaces");

            _WhitespaceWriter.Clear();

            int totalCount = count;

            // do we preserve the tabs or 
            if (convertToSpaces)
                totalCount *= tabSize;

            while (totalCount > 0)
            {
                if (convertToSpaces)
                    _WhitespaceWriter.Append(' ');
                else
                    _WhitespaceWriter.Append('\t');

                totalCount--;
            }

            return _WhitespaceWriter.ToString();
        }

        protected string FindIdField(SqlTable sqlTable)
        {
            // occasionally we might need to figure out a primary key for a table.
            // this method isn't perfect, but its a decent stab at the problem.
            // grab the first PK we have. If there is no PKs defined, grab the
            // first int we find.

            if (sqlTable.PkList.Count == 0)
            {
                foreach (var column in sqlTable.Columns)
                {
                    if (column.Value.BaseType == eSqlBaseType.Integer)
                        return column.Key;
                }

                // still here? no matches then...
                return string.Empty;
            }
            else
            {
                return sqlTable.PkList[0].Name;
            }
        }

        protected string FindNameField(SqlTable sqlTable)
        {
            // occasionally we might need to figure out a friendly name field for 
            // a table. this method isn't perfect, but its a decent stab at the problem.
            // grab the first text field we have, and hope for the best.

            foreach (var column in sqlTable.Columns)
            {
                if (column.Value.BaseType == eSqlBaseType.String)
                    return column.Key;
            }

            // still here? no matches then...
            return string.Empty;
        }

        protected string GenerateNamespaceIncludes(List<string> namespaceIncludes)
        {
            if (namespaceIncludes == null || namespaceIncludes.Count == 0)
                return null;

            var sb = new StringBuilder();

            namespaceIncludes.Sort();

            foreach (var item in namespaceIncludes)
            {
                string buffer = item.Trim();

                if (!string.IsNullOrEmpty(item))
                {
                    if (buffer.Contains(";"))
                        buffer = buffer.Replace(";", string.Empty);

                    sb.AppendLine($"using {buffer};");
                }
            }

            return sb.ToString();
        }
    }
}