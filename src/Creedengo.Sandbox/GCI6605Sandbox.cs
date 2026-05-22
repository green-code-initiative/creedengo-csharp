using System;
using System.Collections.Generic;
using System.Text;

namespace Creedengo.Sandbox;

internal class S6605Sandbox
{

  public bool M1(List<int> ages) => ages.Any(a => IsUnder30(a));
  public bool M2(List<int> ages) => ages.Exists(a => IsUnder30(a));

  private bool IsUnder30(int age) => age < 30;

}
