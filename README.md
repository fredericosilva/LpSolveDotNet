# LpSolveDotNet

LpSolveDotNet is a .NET wrapper  https://github.com/MarcelGosselin/LpSolveDotNet that aims to simplify the code needed to implement this library.

#Example

A store has requested a manufacturer to produce pants and sports jackets.

For materials, the manufacturer has 750 m2 of cotton textile and 1000 m^2 of polyester. Every pair of pants (1 unit) needs 1 m2 of cotton and 2 m2 of polyester. Every jacket needs 1.5 m ^2 of cotton and 1 m^2 of polyester. The price of the pants is fixed at $50 and the jacket, $40. What is the number of pants and jackets that the manufacturer must give to the stores so that these items obtain a maximum sale?

#Code 
```csharp
    Model m = new Model();

    Pants pant = new Pants() { Name = "Foo Pants" };
    Jackets jacket = new Jackets() { Name = "Bar Jackets" };

    var x = m.AddNewVariable<Pants>(pant, p => p.Name, (p, result) => p.OptimalValue = result, (p, till, from) => { p.Till = till; p.From = from; });
    var y = m.AddNewVariable<Jackets>(jacket, j => j.Name, (j, result) => j.OptimalValue = result);

    var objetivo = 50 * x + 40 * y;


    m.AddRestriction(x >= 0, "Positive Pant");
    m.AddRestriction(y >= 0, "Positive Jackets");

    m.AddRestriction(x + 1.5 * y <= 750, "Cotton Textile");
    m.AddRestriction(2 * x + y <= 1000, "Polyester");
    
    m.AddObjetiveFunction(objetive, LinearOptmizationType.Maximizar);

    var r  = m.Run();


    Assert.AreEqual(375, pant.OptimalValue, 0.1);
    Assert.AreEqual(250, jacket.OptimalValue, 0.1);

    Assert.AreEqual(28750, r.OptimizationFunctionResult, 0.1);
    
  
  
```

```csharp
  
  class Pants
    {
        public Pants()
        {
        }

        public string Name { get; set; }
        public double OptimalValue { get; set; }
        public double Till { get; set; }
        public double From { get; set; }
    }
   class Jackets
    {
        public string Name { get; internal set; }

        public double OptimalValue { get; set; }
    }
  
```
