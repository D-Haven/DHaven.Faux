using System;
using System.Text;

namespace DHaven.Faux.Compiler
{
    /// <summary>
    /// Used to keep track of the current indent level so that the class generation stuff is easier to manage
    /// while still providing reasonably indented code to maintain some readability.
    /// </summary>
    public class IndentBuilder : IDisposable
    {
        private readonly StringBuilder builder;
        private readonly int level;
        private bool lineStarted;

        public IndentBuilder()
        {
            builder = new StringBuilder();
            level = 0;
        }

        private IndentBuilder(IndentBuilder parent)
        {
            builder = parent.builder;
            level = parent.level + 1;
        }

        public IndentBuilder Append(string text)
        {
            if (!lineStarted)
            {
                BeginLine();
            }

            builder.Append(text);
            return this;
        }

        public IndentBuilder AppendLine()
        {
            builder.AppendLine();
            lineStarted = false;
            
            return this;
        }

        public IndentBuilder AppendLine(string text)
        {
            if (!lineStarted)
            {
                BeginLine();
            }

            builder.AppendLine(text);
            lineStarted = false;
            
            return this;
        }

        private void BeginLine()
        {
            for (var i = 0; i < level; i++)
            {
                builder.Append("    ");                
            }

            lineStarted = true;
        }

        public IndentBuilder Indent()
        {
            return new IndentBuilder(this);
        }

        public void Dispose()
        {
            // Nothing to do, just decreasing the indent level
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}