using B.Inputs;
using B.Utils;
using Z.Expressions;
using Z.Expressions.Compiler.Shared;

namespace B.Options.ExpressionSolver
{
    public sealed class OptionExpressionSolver : Option<OptionExpressionSolver.Stages>
    {
        public OptionExpressionSolver() : base(Stages.Input) => Input.ResetString();

        public override void Loop()
        {
            switch (this.Stage)
            {
                case Stages.Input:
                    {
                        int consoleWidth = 60;
                        Window.ClearAndSetSize(consoleWidth, 20);
                        Window.PrintLine();
                        Window.PrintLine(" Input:");
                        Window.Print($" {Input.String}");
                        Input.RequestLine(consoleWidth - 2,
                            new Keybind(() => this.SetStage(Stages.Evaluate), key: ConsoleKey.Enter),
                            new Keybind(() => this.Quit(), key: ConsoleKey.Escape));
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
