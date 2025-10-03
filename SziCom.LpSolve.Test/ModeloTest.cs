using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SziCom.LpSolve;

namespace UnitTestProject1
{
    [TestClass]
    public class ModeloTest
    {
        [TestMethod]
        public void SimpleExample()
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

            m.AddObjetiveFuction(objetivo, "Max", LinearOptmizationType.Maximizar);

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
            m2.AddObjetiveFuction(objetivo, "Max", LinearOptmizationType.Maximizar);
            
            modelResult = m2.Run();
            
            Assert.AreEqual(850000.0 + 10 * originalR2Dual, modelResult.OptimizationFunctionResult, 0.1); // New optimal value should reflect the increase in RHS of r2 multiplied by its original dual value
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

            m.AddObjetiveFuction(objetivo, "Max", LinearOptmizationType.Maximizar);

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

            m.AddObjetiveFuction(objetivo, "Max", LinearOptmizationType.Maximizar);

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

            m.AddObjetiveFuction(objetivo, "Max", LinearOptmizationType.Maximizar);

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

            m.AddObjetiveFuction(objetivo, "Max", LinearOptmizationType.Maximizar);

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
            m.AddObjetiveFuction(objetive, LinearOptmizationType.Maximizar);
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
            m.AddObjetiveFuction(objetive, LinearOptmizationType.Maximizar);
            var r = m.Run();


            Assert.AreEqual(SolutionResult.OPTIMAL, r.Tipo);
            Assert.AreEqual(0, x.Result);
            Assert.AreEqual(300, y.Result);
            Assert.AreEqual(0, binaryX.Result);
            Assert.AreEqual(1, binaryY.Result);
        }
    }
}