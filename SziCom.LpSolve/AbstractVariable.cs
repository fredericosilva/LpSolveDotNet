using System;

namespace SziCom.LpSolve
{
    public abstract class AbstractVariable
    {
        public int Index { get; }
        public string Name { get; }

        protected AbstractVariable(int index, string name, bool binary = false)
        {
            this.Index = index;
            this.Name = name;
            this.Binary = binary;
        }

        protected AbstractVariable(int index, Func<string> name, bool binary = false)
        {
            this.Index = index;
            this.Name = name();
            this.Binary = binary;
        }

        internal virtual void SetResult(double result, double from, double till, double dual, double dualFrom, double dualTill)
        {
            this.Result = result;
            this.From = from;
            this.Till = till;
            this.DualValue = dual;
            this.DualFrom = dualFrom;
            this.DualTill = dualTill;
        }

        public double Result { get; private set; }
        
        public double From { get; private set; }
        public double Till { get; private set; }

        public double DualValue { get; private set; }
        public double DualFrom { get; private set; }
        public double DualTill { get; private set; }

        public bool Binary { get; private set; }
        
        public static Term operator +(AbstractVariable a, AbstractVariable b)
        {
            return (Term) a + (Term)b;
        }

        public static Term operator +(double a, AbstractVariable b)
        {
            return a + (Term)b;
        }

        public static Term operator +(AbstractVariable a, double b)
        {
            return (Term)a + b;
        }
        public static Term operator +(int a, AbstractVariable b)
        {
            return a + (Term)b;
        }

        public static Term operator +(AbstractVariable a, int b)
        {
            return (Term)a + b;
        }
        public static Term operator -(AbstractVariable a, AbstractVariable b)
        {
            return (Term)a - (Term)b;
        }
        public static Term operator -(double a, AbstractVariable b)
        {
            return a - (Term)b;
        }
        public static Term operator -(AbstractVariable a, double b)
        {
            return (Term)a - b;
        }
        public static Term operator -(int a, AbstractVariable b)
        {
            return a - (Term)b;
        }
        public static Term operator -(AbstractVariable a, int b)
        {
            return (Term)a - b;
        }
        public static Term operator *(double a, AbstractVariable b)
        {
            return a * (Term)b;
        }

        public static Term operator *(AbstractVariable a, double b)
        {
            return (Term)a * b;
        }
        public static Term operator /(double a, AbstractVariable b)
        {
            return a / (Term)b;
        }

        public static Term operator /(AbstractVariable a, double b)
        {
            return (Term)a / b;
        }
        public static FinalTerm operator >=(AbstractVariable a, double valor)
        {
            return new FinalTerm(a, RestrictionType.GreaterOrEqualThan, valor);
        }
        public static FinalTerm operator <=(AbstractVariable a, double valor)
        {
            return new FinalTerm(a, RestrictionType.LessOrEqualThan, valor);
        }


        public static FinalTerm operator ==(AbstractVariable a, double valor)
        {
            return new FinalTerm(a, RestrictionType.Equals, valor);
        }

        public static FinalTerm operator !=(AbstractVariable a, double valor)
        {
            throw new NotImplementedException();
        }


        public static FinalTerm operator >=(AbstractVariable a, int valor)
        {
            return new FinalTerm(a, RestrictionType.GreaterOrEqualThan, valor);
        }
        public static FinalTerm operator <=(AbstractVariable a, int valor)
        {
            return new FinalTerm(a, RestrictionType.LessOrEqualThan, valor);
        }


        public static FinalTerm operator ==(AbstractVariable a, int valor)
        {
            return new FinalTerm(a, RestrictionType.Equals, valor);
        }

        public static FinalTerm operator !=(AbstractVariable a, int valor)
        {
            throw new NotImplementedException();
        }

        public static implicit operator Term(AbstractVariable variable) 
        {
            return new Term(variable);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                AbstractVariable av = (AbstractVariable)obj;
                return (av.Index == Index) ;
            }
        }

        public override int GetHashCode()
        {
            return Index;
        }
    }
}
