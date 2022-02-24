using B.Inputs;
using B.Utils;
using Z.Expressions;
using Z.Expressions.Compiler.Shared;

namespace B.Options.ExpressionSolver
{
    public sealed class OptionExpressionSolver : Option<OptionExpressionSolver.Stages>
    {
        public OptionExpressionSolver() : base(Stages.Input) => Input.String = string.Empty;

        public override void Loop()
        {
            switch (this.Stage)
            {
                case Stages.Input:
                    {
                        Window.ClearAndSetSize(60, 20);
                        Window.PrintLine();
                        Window.PrintLine(" Input:");
                        Window.Print($" {Input.String}");

                        switch (Input.RequestLine(Window.SIZE_MAX.x).Key) // TODO change from max window width
                        {
                            case ConsoleKey.Escape: this.Quit(); break;
                            case ConsoleKey.Enter: this.SetStage(Stages.Evaluate); break;
                        }
                    }
                    break;

                case Stages.Evaluate:
                    {
                        Window.ClearAndSetSize(40, 15);
                        Window.PrintLine();
                        Window.PrintLine(" Input:");
                        Window.PrintLine($" {Input.String}");
                        Window.PrintLine();
                        Window.PrintLine(" Output:");

                        try { Window.PrintLine($" {Eval.Execute(Input.String)}"); }
                        catch (EvalException) { Window.PrintLine(" Error evaluating expression."); }

                        Input.WaitFor(ConsoleKey.Escape);
                        this.SetStage(Stages.Input);
                    }
                    break;
            }
        }

        public enum Stages
        {
            Input,
            Evaluate,
        }
    }
}
