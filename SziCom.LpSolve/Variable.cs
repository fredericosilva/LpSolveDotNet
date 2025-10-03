using System;

namespace SziCom.LpSolve
{

    public class Variable<T> : AbstractVariable
    {

        public Variable(T valueObject, int index, string name, bool binary = false) : base(index, name, binary)
        {
            this.ValueObject = valueObject;
        }
        
        public Variable(T valueObject, Action<T, double> bindResult, int index, string name, bool binary = false) : this(valueObject, index, name, binary)
        {
            this.BindResult = bindResult;
        }

        public Variable(T valueObject, Action<T, double> result, Action<T, double, double> tillFrom, int index, string name, bool binary = false) : this(valueObject, index, name, binary)
        {
            this.BindResult = result;
            this.BindTillFrom = tillFrom;
        }

        public Variable(T valueObject, Action<T, double> result, Action<T, double, double> tillFrom, int index, Func<T, string> name) : this(valueObject, index, name(valueObject))
        {
            this.BindResult = result;
            this.BindTillFrom = tillFrom;

        }

        public Variable(T valueObject, Action<T, double> result, Action<T, double, double> tillFrom, Action<T, double, double, double> duals, int index, Func<T, string> name) 
            : this(valueObject, result, tillFrom, index, name)
        {
            this.BindDuals = duals;
        }

        internal override void SetResult(double result, double from, double till, double dual, double dualFrom, double dualTill)
        {
            base.SetResult(result, from, till, dual, dualFrom, dualTill);
            BindResult?.Invoke(ValueObject, result);
            BindTillFrom?.Invoke(ValueObject, from, till);
            BindDuals?.Invoke(ValueObject, dual, dualFrom, dualTill);
        }

        public T ValueObject { get; }
        internal Action<T, double> BindResult { get; }
        internal Action<T, double, double> BindTillFrom { get; }
        internal Action<T, double, double, double> BindDuals { get; }


    }
}
