using System.Windows.Forms;
using System.Collections;

namespace Autobook
{
	public class NodeSorter : IComparer
	{
    public int Compare (object x, object y)
		{
			TreeNode tx = x as TreeNode;
			TreeNode ty = y as TreeNode;

      int xOrder;
      int yOrder;

			if (tx.Tag is EventNodeTag)
				xOrder = ((EventNodeTag) tx.Tag).OrderIndex;
			else
				if (tx.Tag is MarketNodeTag)
					xOrder = ((MarketNodeTag) tx.Tag).OrderIndex;
				else
					if (tx.Tag is CountryNodeTag)
						xOrder = ((CountryNodeTag) tx.Tag).OrderIndex;
					else
					{
						System.Diagnostics.Debug.WriteLine ("KO");
						return 1;
					}

			if (ty.Tag is EventNodeTag)
				yOrder = ((EventNodeTag) ty.Tag).OrderIndex;
			else
				if (ty.Tag is MarketNodeTag)
					yOrder = ((MarketNodeTag) ty.Tag).OrderIndex;
				else
					if (ty.Tag is CountryNodeTag)
						yOrder = ((CountryNodeTag) ty.Tag).OrderIndex;
					else
					{
						System.Diagnostics.Debug.WriteLine ("KO");
						return 1;
					}

      return xOrder.CompareTo (yOrder);
		}
	}
}
