using PascalCompiler.Exceptions;
using PascalCompiler.SyntaxAnalyzer.Nodes;

namespace PascalCompiler.Semantics
{
    public class SymStack
    {
        public static readonly SymType SymInt = new SymIntegerType();
        public static readonly SymType SymDouble = new SymDoubleType();
        public static readonly SymType SymChar = new SymCharType();
        public static readonly SymType SymString = new SymStringType();
        public static readonly SymType SymBoolean = new SymBooleanType();

        private Stack<SymTable> _stack;

        public SymStack()
        {
            _stack = new Stack<SymTable>();
            SetBuiltinsTable();
            Push();
        }

        public int Count { get { return _stack.Count; } }

        private void SetBuiltinsTable()
        {
            _stack.Push(new SymTable());
            Add(SymInt);
            Add(SymDouble);
            Add(SymChar);
            Add(SymString);
            Add(SymBoolean);
        }

        public void Push() => _stack.Push(new SymTable());

        public void Push(SymTable table) => _stack.Push(table);

        public SymTable Pop() => _stack.Pop();

        public Symbol Add(Symbol sym)
        {
            _stack.Peek().Add(sym);
            return sym;
        }

        public Symbol AddConst(string symName, SymType type) => Add(new SymConstant(symName, type));

        public Symbol AddVar(string symName, SymType type) => Add(new SymVar(symName, type));

        public Symbol AddAliasType(string symName, SymType type) => Add(new SymAliasType(symName, type));

        public Symbol AddCall(string symName, SymTable @params, SymTable locals, SymType? type = null)
        {
            SymProc symProc = type is null ?
                new SymProc(symName, @params, locals) : new SymFunc(symName, @params, locals, type);

            return Add(symProc);
        }

        public Symbol AddParameter(string symName, SymType type, string modifier) =>
            Add(new SymParameter(symName, type, modifier));

        public void Remove(string symName) => _stack.Peek().Remove(symName.ToLower());

        public Symbol? Find(string symName, bool inScope = false)
        {
            if (inScope)
                return _stack.Peek().Find(symName.ToLower());

            foreach (var table in _stack)
            {
                var symbol = table.Find(symName.ToLower());
                if (symbol is not null)
                    return symbol;
            }

            return null;
        }

        public SymVar? FindIdent(string symName, bool inScope = false)
        {
            var ident = Find(symName, inScope);

            return ident is SymVar ? ident as SymVar : null;
        }

        public SymProc? FindCall(string symName, bool inScope = false) =>
            Find(symName, inScope) switch
            {
                SymFunc symFunc => symFunc,
                SymProc symProc => symProc,
                _ => null,
            };

        public SymType? FindType(string symName, bool inScope = false) =>
            Find(symName, inScope) switch
            {
                SymAliasType symAliasType => symAliasType,
                SymType symType => symType,
                _ => null,
            };


        public void CheckDuplicate(SyntaxNode node)
        {
            if (_stack.Peek().Contains(node.ToString()!.ToLower()))
                throw new SemanticException(node.Lexeme.Pos, $"Duplicate identifier {node}");
        }
    }
}