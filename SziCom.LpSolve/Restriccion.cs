using System;

namespace SziCom.LpSolve
{
    public class Restriccion
    {
        public FinalTerm Termino { get; private set; }
        public string Nombre { get; private set; }
        public int Indice { get; private set; }

        public double Result { get; private set; }

        public double DualValue { get; private set; }
        public double DualFrom { get; private set; }
        public double DualTill { get; private set; }

        internal Action<double> BindResult { get; }
        internal Action<double, double, double> BindDuals { get; }

        public Restriccion(FinalTerm termino, string nombre, int indice)
        {
            this.Termino = termino;
            this.Nombre = nombre;
            this.Indice = indice;
        }

        public Restriccion(FinalTerm termino, string nombre, int indice, Action<double> bindResult)
            : this(termino, nombre, indice)
        {
            this.BindResult = bindResult;
        }

        public Restriccion(FinalTerm termino, string nombre, int indice, Action<double> bindResult, Action<double, double, double> bindDuals)
            : this(termino, nombre, indice, bindResult)
        {
            this.BindDuals = bindDuals;
        }

        internal void SetResult(double result, double dual, double dualFrom, double dualTill)
        {
            this.Result = result;
            this.DualValue = dual;
            this.DualFrom = dualFrom;
            this.DualTill = dualTill;

            BindResult?.Invoke(result);
            BindDuals?.Invoke(dual, dualFrom, dualTill);
        }
    }
}