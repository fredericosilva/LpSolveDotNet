using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SziCom.LpSolve;

namespace UnitTestProject1
{
    [TestClass]
    public class ModeloTest
    {
        [TestMethod]
        public void OptimalResultImprovement_ShadowPriceVariation_MaximizationProblem()
        {
            Model m = new Model();
            var x = m.AddNewVariable<string>("", "X");
            var y = m.AddNewVariable<string>("", "Y");

            var r1 = x + 2.0 * y <= 80;
            var r2 = 3 * x + 2 * y <= 120;

            var r3 = x >= 0;
            var r4 = y >= 0;

            var objetivo = 20000 * x + 15000 * y;

            // Variables to store bound constraint results and duals
            double r1Result = 0, r1Dual = 0, r1DualFrom = 0, r1DualTill = 0;
            double r2Result = 0, r2Dual = 0, r2DualFrom = 0, r2DualTill = 0;
            double r3Result = 0, r3Dual = 0, r3DualFrom = 0, r3DualTill = 0;
            double r4Result = 0, r4Dual = 0, r4DualFrom = 0, r4DualTill = 0;

            // Add restrictions with binding for results and duals
            m.AddRestriction(r1, "R1", result => r1Result = result, (dual, dualFrom, dualTill) => { r1Dual = dual; r1DualFrom = dualFrom; r1DualTill = dualTill; });
            m.AddRestriction(r2, "R2", result => r2Result = result, (dual, dualFrom, dualTill) => { r2Dual = dual; r2DualFrom = dualFrom; r2DualTill = dualTill; });
            m.AddRestriction(r3, "R3", result => r3Result = result, (dual, dualFrom, dualTill) => { r3Dual = dual; r3DualFrom = dualFrom; r3DualTill = dualTill; });
            m.AddRestriction(r4, "R4", result => r4Result = result, (dual, dualFrom, dualTill) => { r4Dual = dual; r4DualFrom = dualFrom; r4DualTill = dualTill; });

            m.AddObjetiveFunction(objetivo, "Max", LinearOptmizationType.Maximizar);

            var modelResult = m.Run();

            Assert.AreEqual(850000.0, modelResult.OptimizationFunctionResult, 0.1);

            Assert.AreEqual(20.0, x.Result, 0.1);
            Assert.AreEqual(30.0, y.Result, 0.1);

            // "Reduced costs" (dual values) for variables must be 0 if they are used in the solution
            Assert.IsTrue(x.DualValue == 0);
            Assert.IsTrue(y.DualValue == 0);

            // Assertions for constraint results (slack/surplus values)
            Assert.AreEqual(80.0, r1Result, 0.1, "R1 constraint should be tight");
            Assert.AreEqual(120.0, r2Result, 0.1, "R2 constraint should be tight");
            Assert.AreEqual(20.0, r3Result, 0.1, "R3 constraint result should equal X value");
            Assert.AreEqual(30.0, r4Result, 0.1, "R4 constraint result should equal Y value");

            // "Shadow prices" (dual values) with from-till limits for constraints that are tight
            Assert.AreEqual(1250.0, r1Dual, 0.1);
            Assert.AreEqual(40.0, r1DualFrom, 0.1);
            Assert.AreEqual(120.0, r1DualTill, 0.1);

            Assert.AreEqual(6250.0, r2Dual, 0.1);
            Assert.AreEqual(80.0, r2DualFrom, 0.1);
            Assert.AreEqual(240.0, r2DualTill, 0.1);

            // Assert OptimizationFunctionResult variation (based on duals) when restriction RHS changes
            var originalR2Dual = r2Dual;
            
            Model m2 = new Model();
            r2 = 3 * x + 2 * y <= 130; // Increase RHS of r2 by 10, still on duals from-till range

            m2.AddNewVariable<string>("", "X");
            m2.AddNewVariable<string>("", "Y");
            m2.AddRestriction(r1, "R1", result => r1Result = result, (dual, dualFrom, dualTill) => { r1Dual = dual; r1DualFrom = dualFrom; r1DualTill = dualTill; });
            m2.AddRestriction(r2, "R2", result => r2Result = result, (dual, dualFrom, dualTill) => { r2Dual = dual; r2DualFrom = dualFrom; r2DualTill = dualTill; });
            m2.AddRestriction(r3, "R3", result => r3Result = result, (dual, dualFrom, dualTill) => { r3Dual = dual; r3DualFrom = dualFrom; r3DualTill = dualTill; });
            m2.AddRestriction(r4, "R4", result => r4Result = result, (dual, dualFrom, dualTill) => { r4Dual = dual; r4DualFrom = dualFrom; r4DualTill = dualTill; });
            m2.AddObjetiveFunction(objetivo, "Max", LinearOptmizationType.Maximizar);
            
            modelResult = m2.Run();
            
            Assert.AreEqual(850000.0 + 10 * originalR2Dual, modelResult.OptimizationFunctionResult, 0.1); // New optimal value should reflect the increase in RHS of r2 multiplied by its original dual value
        }

        [TestMethod]
        public void OptimalResultImprovement_ShadowPriceVariation_MinimizationProblem()
        {
            // First model
            Model m1 = new Model();
            var x1 = m1.AddNewVariable<string>("", "X1");
            var y1 = m1.AddNewVariable<string>("", "Y1");

            var capacity1 = 2 * x1 + y1 <= 100;
            var demand1 = x1 + y1 >= 60;
            var nonNegX1 = x1 >= 0;
            var nonNegY1 = y1 >= 0;

            var objective1 = 5 * x1 + 8 * y1;

            double capResult1 = 0, capDual1 = 0, capDualFrom1 = 0, capDualTill1 = 0;
            double demandResult1 = 0;

            m1.AddRestriction(capacity1, "Capacity", r => capResult1 = r, (dual, from, till) => { capDual1 = dual; capDualFrom1 = from; capDualTill1 = till; });
            m1.AddRestriction(demand1, "Demand", r => demandResult1 = r);
            m1.AddRestriction(nonNegX1, "NonNegX1");
            m1.AddRestriction(nonNegY1, "NonNegY1");
            m1.AddObjetiveFunction(objective1, "Min", LinearOptmizationType.Minimizar);

            var result1 = m1.Run();

            Assert.AreEqual(SolutionResult.OPTIMAL, result1.Tipo);
            Assert.AreEqual(40.0, x1.Result, 0.1);
            Assert.AreEqual(20.0, y1.Result, 0.1);
            Assert.AreEqual(100.0, capResult1, 0.1, "Capacity constraint should be tight");
            Assert.AreEqual(60.0, demandResult1, 0.1, "Demand constraint should be tight");
            Assert.AreEqual(360.0, result1.OptimizationFunctionResult, 0.1);

            Assert.IsTrue(capDual1 < 0, "Capacity shadow price should be negative (relaxing the constraint lowers cost).");

            var originalResult = result1.OptimizationFunctionResult;
            var originalCapacityDual = capDual1;

            // Second model with increased capacity RHS by 10 (from 100 to 110)
            Model m2 = new Model();
            var x2 = m2.AddNewVariable<string>("", "X1");
            var y2 = m2.AddNewVariable<string>("", "Y1");

            var capacity2 = 2 * x2 + y2 <= 110; // Relaxed by +10
            var demand2 = x2 + y2 >= 60;
            var nonNegX2 = x2 >= 0;
            var nonNegY2 = y2 >= 0;

            var objective2 = 5 * x2 + 8 * y2;

            double capResult2 = 0, capDual2 = 0, capDualFrom2 = 0, capDualTill2 = 0;
            double demandResult2 = 0;

            m2.AddRestriction(capacity2, "Capacity", r => capResult2 = r, (dual, from, till) => { capDual2 = dual; capDualFrom2 = from; capDualTill2 = till; });
            m2.AddRestriction(demand2, "Demand", r => demandResult2 = r);
            m2.AddRestriction(nonNegX2, "NonNegX1");
            m2.AddRestriction(nonNegY2, "NonNegY1");
            m2.AddObjetiveFunction(objective2, "Min", LinearOptmizationType.Minimizar);

            var result2 = m2.Run();

            Assert.AreEqual(SolutionResult.OPTIMAL, result2.Tipo);
            Assert.AreEqual(50.0, x2.Result, 0.1);
            Assert.AreEqual(10.0, y2.Result, 0.1);
            Assert.AreEqual(110.0, capResult2, 0.1, "New capacity constraint should be tight");
            Assert.AreEqual(60.0, demandResult2, 0.1, "Demand constraint should remain tight");
            Assert.AreEqual(330.0, result2.OptimizationFunctionResult, 0.1);

            // Predicted improvement using original dual (delta = +10)
            var expectedImprovedObjective = originalResult + 10 * originalCapacityDual;
            Assert.AreEqual(expectedImprovedObjective, result2.OptimizationFunctionResult, 0.1, "Objective should change by delta * shadow price when RHS varies within allowable range.");

            Assert.IsTrue(result2.OptimizationFunctionResult < originalResult, "Objective should decrease (improve) after relaxing capacity.");
        }

        [TestMethod]
        public void OptimalResultGetWorse_ReducedCostVariation_MinimizationProblem()
        {
            // First model - standard minimization problem
            Model m1 = new Model();
            var x1 = m1.AddNewVariable<string>("", "X1");
            var x2 = m1.AddNewVariable<string>("", "X2");
            var x3 = m1.AddNewVariable<string>("", "X3");

            // Constraints: ensure we have slack so some variables won't be in optimal solution
            var r1 = x1 + x2 + x3 >= 10;  // Minimum production requirement
            var r2 = 2 * x1 + x2 <= 30;   // Resource constraint 1
            var r3 = x1 + 3 * x2 <= 40;   // Resource constraint 2

            // Non-negativity constraints
            var r4 = x1 >= 0;
            var r5 = x2 >= 0;
            var r6 = x3 >= 0;

            // Objective: minimize cost (x3 has higher cost, should have positive reduced cost)
            var objetivo = 2 * x1 + 3 * x2 + 10 * x3;

            m1.AddRestriction(r1, "MinProduction");
            m1.AddRestriction(r2, "Resource1");
            m1.AddRestriction(r3, "Resource2");
            m1.AddRestriction(r4, "NonNegX1");
            m1.AddRestriction(r5, "NonNegX2");
            m1.AddRestriction(r6, "NonNegX3");

            m1.AddObjetiveFunction(objetivo, "Min", LinearOptmizationType.Minimizar);

            var result1 = m1.Run();

            // Store original results
            var originalOptimalValue = result1.OptimizationFunctionResult;
            var originalX1 = x1.Result;
            var originalX2 = x2.Result;
            var originalX3 = x3.Result;
            var x3ReducedCost = x3.DualValue; // Should be positive since x3 is expensive and not used

            // Verify x3 is not in the solution (should be 0) and has positive reduced cost
            Assert.AreEqual(0.0, originalX3, 0.1, "X3 should not be in optimal solution");
            Assert.IsTrue(x3ReducedCost > 0, "X3 should have positive reduced cost in minimization");

            // Second model - force x3 into solution by requiring minimum of 1 unit
            Model m2 = new Model();
            var x1_m2 = m2.AddNewVariable<string>("", "X1");
            var x2_m2 = m2.AddNewVariable<string>("", "X2");
            var x3_m2 = m2.AddNewVariable<string>("", "X3");

            // Same constraints as before
            var r1_m2 = x1_m2 + x2_m2 + x3_m2 >= 10;
            var r2_m2 = 2 * x1_m2 + x2_m2 <= 30;
            var r3_m2 = x1_m2 + 3 * x2_m2 <= 40;
            var r4_m2 = x1_m2 >= 0;
            var r5_m2 = x2_m2 >= 0;
            var r6_m2 = x3_m2 >= 1; // Force at least 1 unit of x3

            var objetivo_m2 = 2 * x1_m2 + 3 * x2_m2 + 10 * x3_m2;

            m2.AddRestriction(r1_m2, "MinProduction");
            m2.AddRestriction(r2_m2, "Resource1");
            m2.AddRestriction(r3_m2, "Resource2");
            m2.AddRestriction(r4_m2, "NonNegX1");
            m2.AddRestriction(r5_m2, "NonNegX2");
            m2.AddRestriction(r6_m2, "ForceX3Min"); // This forces x3 into solution

            m2.AddObjetiveFunction(objetivo_m2, "Min", LinearOptmizationType.Minimizar);

            var result2 = m2.Run();

            // Verify that forcing 1 unit of x3 increases the optimal value by the reduced cost
            var expectedNewOptimalValue = originalOptimalValue + (1.0 * x3ReducedCost);

            Assert.AreEqual(expectedNewOptimalValue, result2.OptimizationFunctionResult, 0.1,
                "New optimal value should increase by the reduced cost of x3 when forced into solution");

            Assert.AreEqual(1.0, x3_m2.Result, 0.1, "X3 should be exactly 1 in the new solution");

            // Verify the solution is still optimal
            Assert.AreEqual(SolutionResult.OPTIMAL, result2.Tipo, "Second model should also be optimal");
        }

        [TestMethod]
        public void OptimalResultImprovement_ReducedCostCoeficientVariation_MinimizationProblem()
        {
            // Goal: show a minimization where a currently unused variable has a positive reduced cost, and after
            // decreasing its cost coefficient it enters the solution and improves (lowers) the objective value.

            // First model
            Model m1 = new Model();
            var x1 = m1.AddNewVariable<string>("", "X1"); // Cheaper activity (cost 5)
            var x2 = m1.AddNewVariable<string>("", "X2"); // More expensive alternative (cost 10)

            // Demand constraint: need at least 10 units in total
            var demand1 = x1 + x2 >= 10;
            var nonNegX1_1 = x1 >= 0;
            var nonNegX2_1 = x2 >= 0;

            var objective1 = 5 * x1 + 10 * x2; // Minimize cost

            m1.AddRestriction(demand1, "Demand");
            m1.AddRestriction(nonNegX1_1, "NonNegX1");
            m1.AddRestriction(nonNegX2_1, "NonNegX2");
            m1.AddObjetiveFunction(objective1, "Min", LinearOptmizationType.Minimizar);

            var result1 = m1.Run();

            // Expected: use only X1 (cheaper) to satisfy demand
            Assert.AreEqual(SolutionResult.OPTIMAL, result1.Tipo);
            Assert.AreEqual(10.0, x1.Result, 0.1);
            Assert.AreEqual(0.0, x2.Result, 0.1);

            // Reduced cost for unused variable in minimization should be positive
            var x2ReducedCost = x2.DualValue; // How much worse objective becomes per unit of X2 forced in (at current coefficients)
            Assert.IsTrue(x2ReducedCost > 0, "X2 reduced cost should be positive while it is nonbasic at zero.");

            var originalObjective = result1.OptimizationFunctionResult; // Should be 50 (5 * 10)
            Assert.AreEqual(50.0, originalObjective, 0.1);

            // Second model: reduce cost of X2 so it becomes attractive and enters solution
            Model m2 = new Model();
            var x1_m2 = m2.AddNewVariable<string>("", "X1");
            var x2_m2 = m2.AddNewVariable<string>("", "X2");

            var demand2 = x1_m2 + x2_m2 >= 10;
            var nonNegX1_2 = x1_m2 >= 0;
            var nonNegX2_2 = x2_m2 >= 0;

            // Lower cost of X2 from 10 to 2 (now cheaper than X1)
            var objective2 = 5 * x1_m2 + 2 * x2_m2;

            m2.AddRestriction(demand2, "Demand");
            m2.AddRestriction(nonNegX1_2, "NonNegX1");
            m2.AddRestriction(nonNegX2_2, "NonNegX2");
            m2.AddObjetiveFunction(objective2, "Min", LinearOptmizationType.Minimizar);

            var result2 = m2.Run();

            Assert.AreEqual(SolutionResult.OPTIMAL, result2.Tipo);

            // Now X2 should satisfy all demand because it became the cheapest
            Assert.AreEqual(0.0, x1_m2.Result, 0.1);
            Assert.AreEqual(10.0, x2_m2.Result, 0.1);

            // Objective should improve (decrease) from 50 to 20
            Assert.AreEqual(20.0, result2.OptimizationFunctionResult, 0.1);
            Assert.IsTrue(result2.OptimizationFunctionResult < originalObjective, "Objective should decrease after lowering X2 cost.");

            // After entering, reduced cost of X2 must become ~0
            Assert.AreEqual(0.0, x2_m2.DualValue, 0.1, "Reduced cost of a basic/used variable should be ~0.");
        }

        [TestMethod]
        public void SimpleExampleWithSumConstantes()
        {
            Model m = new Model();
            var x = m.AddNewVariable<string>("", "X");
            var y = m.AddNewVariable<string>("", "Y");

            var r1 = x + 2.0 * y + 10 <= 90;
            var r2 = 3 * x + 2 * y <= 120;

            var r3 = x >= 0;
            var r4 = y >= 0;

            var objetivo = 20 * x + 15 * y;

            m.AddRestriction(r1, "R1");
            m.AddRestriction(r2, "R1");
            m.AddRestriction(r3, "R1");
            m.AddRestriction(r4, "R1");

            m.AddObjetiveFunction(objetivo, "Max", LinearOptmizationType.Maximizar);

            m.Run();

            Assert.AreEqual(20, x.Result, .1);
            Assert.AreEqual(30, y.Result, .1);
        }
        [TestMethod]
        public void SimpleExampleWithRestaConstantes()
        {
            Model m = new Model();
            var x = m.AddNewVariable<string>("", "X");
            var y = m.AddNewVariable<string>("", "Y");

            var r1 = x + 2.0 * y - 5 <= 75;
            var r2 = 3 * x + 2 * y <= 120;

            var r3 = x >= 0;
            var r4 = y >= 0;

            var objetivo = 20 * x + 15 * y;

            m.AddRestriction(r1, "R1");
            m.AddRestriction(r2, "R1");
            m.AddRestriction(r3, "R1");
            m.AddRestriction(r4, "R1");

            m.AddObjetiveFunction(objetivo, "Max", LinearOptmizationType.Maximizar);

            m.Run();

            Assert.AreEqual(20, x.Result, .1);
            Assert.AreEqual(30, y.Result, .1);
        }
        [TestMethod]
        public void ScalingExample()
        {
            Model m = new Model();
            var x = m.AddNewVariable<string>("", "X");
            var y = m.AddNewVariable<string>("", "Y");

            var r1 = x + 2.0 * y <= 80;
            var r2 = 3 * x + 2 * y <= 120;

            var r3 = x >= 0;
            var r4 = y >= 0;

            var objetivo = 20000 * x + 15000 * y;

            m.AddRestriction(r1, "R1");
            m.AddRestriction(r2, "R1");
            m.AddRestriction(r3, "R1");
            m.AddRestriction(r4, "R1");

            m.AddObjetiveFunction(objetivo, "Max", LinearOptmizationType.Maximizar);

            m.Run(100);

            Assert.AreEqual(20.0, x.Result, 0.1);
            Assert.AreEqual(30.0, y.Result, 0.1);
        }

        [TestMethod]
        public void SumExample()
        {
            Model m = new Model();
            var x = m.AddNewVariables<string>(new List<string>() { "" }, (t) => "X");
            var y = m.AddNewVariable<string>("Hola", "Y", (t, resultado) => System.Diagnostics.Debug.WriteLine(t + resultado));

            var r1 = m.SumWhere(t => true, x) + 2.0 * y <= 80;
            var r2 = 3 * m.Sum(x) + 2 * y <= 120;

            foreach (var item in x)
            {
                m.AddRestriction(item >= 0, "R1");
            }

            var r4 = y >= 0;

            var objetivo = 20000 * m.Sum(x) + 15000 * y;

            m.AddRestriction(r1, "R1");
            m.AddRestriction(r2, "R1");

            m.AddRestriction(r4, "R1");

            m.AddObjetiveFunction(objetivo, "Max", LinearOptmizationType.Maximizar);

            m.Run();

            foreach (var item in x)
            {
                Assert.AreEqual(20.0, item.Result, 0.1);
            }

            Assert.AreEqual(30.0, y.Result, 0.1);
        }


        [TestMethod]
        public void VariableExample()
        {
            Model m = new Model();

            Pants pant = new Pants() { Name = "Foo Pants" };
            Jackets jacket = new Jackets() { Name = "Bar Jackets" };

            var x = m.AddNewVariable<Pants>(pant, p => p.Name, (p, result) => p.OptimalValue = result, (p, till, from) => { p.Till = till; p.From = from; });
            var y = m.AddNewVariable<Jackets>(jacket, j => j.Name, (j, result) => j.OptimalValue = result);



            var objetive = 50 * x + 40 * y;

            m.AddRestriction(x >= 0, "Positive Pant");
            m.AddRestriction(y >= 0, "Positive Jackets");

            m.AddRestriction(x + 1.5 * y <= 750, "Contton Textile");
            m.AddRestriction(2 * x + y <= 1000, "Polyester");
            m.AddObjetiveFunction(objetive, LinearOptmizationType.Maximizar);
            var r = m.Run(100);


            Assert.AreEqual(375, pant.OptimalValue, 0.1);
            Assert.AreEqual(250, jacket.OptimalValue, 0.1);

            Assert.AreEqual(28750, r.OptimizationFunctionResult, 0.1);
        }

        [TestMethod]
        public void BinaryExample()
        {
            Model m = new Model();


            var x = m.AddNewVariable<string>("X", "X");
            var y = m.AddNewVariable<string>("Y", "Y");
            var binaryX = m.AddNewVariable<string>("B", "B", true);
            var binaryY = m.AddNewVariable<string>("B", "B", true);

            var objetive = 1.5 * x + 2 * y;

            m.AddRestriction(x <= 300, "");
            m.AddRestriction(y <= 300, "");

            m.AddRestriction(x - 1000 * binaryX <= 0, "Contton Textile");
            m.AddRestriction(x + 1000 * binaryY >= 10, "Polyester");
            m.AddRestriction(binaryX + binaryY == 1, "Polyester");
            m.AddObjetiveFunction(objetive, LinearOptmizationType.Maximizar);
            var r = m.Run();


            Assert.AreEqual(SolutionResult.OPTIMAL, r.Tipo);
            Assert.AreEqual(0, x.Result);
            Assert.AreEqual(300, y.Result);
            Assert.AreEqual(0, binaryX.Result);
            Assert.AreEqual(1, binaryY.Result);
        }
    }
}