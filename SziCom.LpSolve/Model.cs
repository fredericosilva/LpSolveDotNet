using LpSolveDotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SziCom.LpSolve
{
    public class Model
    {
        private readonly Dictionary<int, AbstractVariable> Variables = new Dictionary<int, AbstractVariable>();
        public readonly Dictionary<int, Restriccion> Restricciones = new Dictionary<int, Restriccion>();
        public LinearOptimizacionFunction Objetivo { get; private set; }
        public int RestrictionIndex { get; private set; }
        public int VariablesIndex { get; private set; }

        public Variable<T> AddNewVariable<T>(T valueObject, string nombre, Action<T, double> bind, bool binary = false) where T : class
        {
            var variable = new Variable<T>(valueObject, bind, ++VariablesIndex, nombre, binary);
            Variables.Add(variable.Index, variable);
            return variable;
        }
        public Variable<T> AddNewVariable<T>(T valueObject, string nombre, Action<T, double> bind) where T : class
        {
            var variable = new Variable<T>(valueObject, bind, ++VariablesIndex, nombre);
            Variables.Add(variable.Index, variable);
            return variable;
        }

        public Variable<T> AddNewVariable<T>(T valueObject, Func<T, string> nombre, Action<T, double> bind) where T : class
        {
            var variable = new Variable<T>(valueObject, bind, ++VariablesIndex, nombre(valueObject));
            Variables.Add(variable.Index, variable);
            return variable;
        }
        public Variable<T> AddNewVariable<T>(T valueObject, Func<T, string> nombre, Action<T, double> result, Action<T, double, double> tillFrom) where T : class
        {
            var variable = new Variable<T>(valueObject, result, tillFrom, ++VariablesIndex, nombre);
            Variables.Add(variable.Index, variable);
            return variable;
        }


        public Variable<T> AddNewVariable<T>(T valueObject, string nombre, bool binary = false) where T : class
        {
            var variable = new Variable<T>(valueObject, ++VariablesIndex, nombre, binary);
            Variables.Add(variable.Index, variable);
            return variable;
        }

        public IEnumerable<Variable<T>> AddNewVariables<T>(IEnumerable<T> collection, Func<T, string> nameFunction)
        {
            var result = new List<Variable<T>>();
            foreach (var item in collection)
            {
                var variable = new Variable<T>(item, ++VariablesIndex, nameFunction(item));
                Variables.Add(variable.Index, variable);
                result.Add(variable);
            }
            return result;
        }
        public IEnumerable<Variable<T>> AddNewVariables<T>(IEnumerable<T> collection, Func<T, string> nameFunction, Action<T, double> bindResult)
        {
            var result = new List<Variable<T>>();
            foreach (var item in collection)
            {
                var variable = new Variable<T>(item, bindResult, ++VariablesIndex, nameFunction(item));
                Variables.Add(variable.Index, variable);
                result.Add(variable);
            }
            return result;
        }

        public IEnumerable<Variable<T>> AddNewVariables<T>(IEnumerable<T> collection, Func<T, string> nameFunction, Action<T, double> bindResult, Action<T, double, double> tillFrom)
        {
            var result = new List<Variable<T>>();
            foreach (var item in collection)
            {
                var variable = new Variable<T>(item, bindResult, tillFrom, ++VariablesIndex, nameFunction(item));
                Variables.Add(variable.Index, variable);
                result.Add(variable);
            }
            return result;
        }

        public IEnumerable<Variable<T>> AddNewVariables<T>(IEnumerable<T> collection, Func<T, string> nameFunction, Action<T, double> bindResult, Action<T, double, double> tillFrom, Action<T, double, double, double> duals)
        {
            var result = new List<Variable<T>>();
            foreach (var item in collection)
            {
                var variable = new Variable<T>(item, bindResult, tillFrom, duals, ++VariablesIndex, nameFunction);
                Variables.Add(variable.Index, variable);
                result.Add(variable);
            }
            return result;
        }

        public void AddObjetiveFunction(Term term, string nombreRestriccion, LinearOptmizationType funcion)
        {
            this.Objetivo = new LinearOptimizacionFunction(term, nombreRestriccion, funcion);
        }
        public void AddObjetiveFunction(Term term, LinearOptmizationType funcion)
        {
            this.Objetivo = new LinearOptimizacionFunction(term, "OptimizationFunctionName", funcion);
        }

        public void AddRestriction(FinalTerm terms, string nombreRestriccion)
        {
            var restriccion = new Restriccion(terms, nombreRestriccion, ++RestrictionIndex);
            Restricciones.Add(restriccion.Indice, restriccion);
        }
        public void AddRestriction(FinalTerm terms, string nombreRestriccion, Action<double> bindResult)
        {
            var restriccion = new Restriccion(terms, nombreRestriccion, ++RestrictionIndex, bindResult);
            Restricciones.Add(restriccion.Indice, restriccion);
        }
        public void AddRestriction(FinalTerm terms, string nombreRestriccion, Action<double> bindResult, Action<double, double, double> bindDuals)
        {
            var restriccion = new Restriccion(terms, nombreRestriccion, ++RestrictionIndex, bindResult, bindDuals);
            Restricciones.Add(restriccion.Indice, restriccion);
        }

        public Term Restar<T>(IEnumerable<Variable<T>> variables)
        {
            return new Term(variables.ToDictionary(v => (AbstractVariable)v, v => new InternalFactor(-1.0)));
        }

        public Result Run(double scale = 1.0)
        {
            LpSolveDotNet.LpSolve.Init();


            using (LpSolveDotNet.LpSolve lp = LpSolveDotNet.LpSolve.make_lp(0, VariablesIndex))
            {

                lp.set_scaling(lpsolve_scale_algorithm.SCALE_EXTREME, lpsolve_scale_parameters.SCALE_EQUILIBRATE);

                AddRestrictions(lp, scale);
                RenameLPSolveColumns(lp);
                AddObjetiveFunction(lp, scale);
                SetBinaryVaraibles(lp);

                ///Importante: El resultado no se escala, porque al escalar los coeficientes de las variables 
                ///y de la fucion objetivo, el resutaldo de la funcion objetivo ya esta correcto.

                //lp.set_presolve()

                var result = new Result(ToLpSolveContraintType(lp.solve()), Math.Round(lp.get_objective(), 2));

                Double[] varResult = new Double[lp.get_Ncolumns()];
                Double[] varFrom = new Double[lp.get_Ncolumns()];
                Double[] varTill = new Double[lp.get_Ncolumns()];

                Double[] constraintResult = new Double[lp.get_Nrows()];

                Double[] duals = new Double[lp.get_Nrows() + lp.get_Ncolumns()];
                Double[] fromDuals = new Double[lp.get_Nrows() + lp.get_Ncolumns()];
                Double[] tillDuals = new Double[lp.get_Nrows() + lp.get_Ncolumns()];

                lp.get_variables(varResult);
                lp.get_constraints(constraintResult);
                lp.get_sensitivity_obj(varFrom, varTill);
                lp.get_sensitivity_rhs(duals, fromDuals, tillDuals);

                foreach (var v in Variables)
                {
                    v.Value.SetResult(varResult[v.Key - 1] / scale,
                                      varFrom[v.Key - 1] / scale,
                                      varTill[v.Key - 1] / scale,
                                      duals[lp.get_Nrows() + v.Key - 1] / scale,
                                      fromDuals[lp.get_Nrows() + v.Key - 1] / scale,
                                      tillDuals[lp.get_Nrows() + v.Key - 1] / scale);
                }

                foreach (var r in Restricciones)
                {
                    r.Value.SetResult(constraintResult[r.Key - 1] / scale, duals[r.Key - 1] / scale, fromDuals[r.Key - 1] / scale, tillDuals[r.Key - 1] / scale);
                }

                return result;
            }
        }

        private void SetBinaryVaraibles(LpSolveDotNet.LpSolve lp)
        {

            foreach (var v in Variables.Where(t => t.Value.Binary))
            {
                lp.set_binary(v.Key, true);
            }
        }

        private void RenameLPSolveColumns(LpSolveDotNet.LpSolve lp)
        {
            foreach (var v in Variables)
            {
                lp.set_col_name(v.Key, v.Value.Name);
            }
        }

        public Term Sum<T>(IEnumerable<Variable<T>> variables)
        {
            return new Term(variables.ToDictionary(v => (AbstractVariable)v, v => new InternalFactor()));
        }

        public Term SumWhere<T>(Func<Variable<T>, bool> conditional, IEnumerable<Variable<T>> variables)
        {
            return new Term(variables.Where(conditional).ToDictionary(v => (AbstractVariable)v, v => new InternalFactor()));
        }
        private void AddObjetiveFunction(LpSolveDotNet.LpSolve lp, double scale)
        {
            Int32[] variables = Objetivo.FunctionTerm.GetVariables();
            Double[] coeficientes = Objetivo.FunctionTerm.GetCoeficientes().Select(c => c / scale).ToArray();

            if (!lp.set_obj_fnex(Objetivo.FunctionTerm.Count, coeficientes, variables))
            {
                throw new LpSolveExeption($"Restricciones Factory: Error al agregar la restriccion: {Objetivo.Name}");
            }
            if (Objetivo.Tipo == LinearOptmizationType.Maximizar)
            {
                lp.set_maxim();
            }

            if (Objetivo.Tipo == LinearOptmizationType.Minimizar) lp.set_minim();
        }

        private void AddRestrictions(LpSolveDotNet.LpSolve lp, double scale)
        {
            foreach (var r in Restricciones)
            {
                Int32[] variables = r.Value.Termino.GetVariables();
                Double[] coeficientes = r.Value.Termino.GetCoeficientes().Select(t => t / scale).ToArray();

                if (!lp.add_constraintex(r.Value.Termino.Count, coeficientes, variables, ToLpSolveContraintType(r.Value.Termino.Restriction), r.Value.Termino.RestrictionValue - r.Value.Termino.GetAdding()))
                {
                    throw new LpSolveExeption($"Restricciones Factory: Error al agregar la restriccion: {r.Value.Nombre}");
                }

                lp.set_row_name(r.Key, r.Value.Nombre);
            }
        }

        private lpsolve_constr_types ToLpSolveContraintType(RestrictionType restriccion)
        {
            switch (restriccion)
            {
                case RestrictionType.Equals:
                    return lpsolve_constr_types.EQ;

                case RestrictionType.GreaterOrEqualThan:
                    return lpsolve_constr_types.GE;

                case RestrictionType.LessOrEqualThan:
                    return lpsolve_constr_types.LE;
            }

            throw new InvalidOperationException();
        }

        private SolutionResult ToLpSolveContraintType(lpsolve_return tipo)
        {
            switch (tipo)
            {
                case lpsolve_return.UNKNOWNERROR: return SolutionResult.UNKNOWNERROR;
                case lpsolve_return.DATAIGNORED: return SolutionResult.DATAIGNORED;
                case lpsolve_return.NOBFP: return SolutionResult.NOBFP;
                case lpsolve_return.NOMEMORY: return SolutionResult.NOMEMORY;
                case lpsolve_return.NOTRUN: return SolutionResult.NOTRUN;
                case lpsolve_return.OPTIMAL: return SolutionResult.OPTIMAL;
                case lpsolve_return.SUBOPTIMAL: return SolutionResult.SUBOPTIMAL;
                case lpsolve_return.INFEASIBLE: return SolutionResult.INFEASIBLE;
                case lpsolve_return.UNBOUNDED: return SolutionResult.UNBOUNDED;
                case lpsolve_return.DEGENERATE: return SolutionResult.DEGENERATE;
                case lpsolve_return.NUMFAILURE: return SolutionResult.NUMFAILURE;
                case lpsolve_return.USERABORT: return SolutionResult.USERABORT;
                case lpsolve_return.TIMEOUT: return SolutionResult.TIMEOUT;
                case lpsolve_return.PRESOLVED: return SolutionResult.PRESOLVED;
                case lpsolve_return.ACCURACYERROR: return SolutionResult.ACCURACYERROR;
            }
            throw new InvalidOperationException();
        }
    }
}