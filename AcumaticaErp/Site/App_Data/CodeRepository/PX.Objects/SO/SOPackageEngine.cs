using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System.Diagnostics;

namespace PX.Objects.SO
{
	public class SOPackageEngine
	{
		protected PXGraph graph;

		public SOPackageEngine(PXGraph graph)
		{
			if (graph == null)
				throw new ArgumentNullException("graph");

			this.graph = graph;
		}


		public virtual IList<PackSet> Pack(OrderInfo orderInfo)
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();

			Dictionary<int, List<INItemBoxEx>> boxesByInventory = new Dictionary<int, List<INItemBoxEx>>();
			
			foreach (ItemInfo itemInfo in orderInfo.Lines)
			{
				boxesByInventory.Add( itemInfo.InventoryID, GetBoxesByInventoryID(itemInfo.InventoryID, orderInfo.CarrierID));
			}
			
			Dictionary<int, PackSet> list = new Dictionary<int, PackSet>();

			foreach (ItemStats stat in orderInfo.Stats)
			{
				PackSet ps = null;
				if (list.ContainsKey(stat.SiteID.Value))
				{
					ps = list[stat.SiteID.Value];
				}
				else
				{
					ps = new PackSet(stat.SiteID.Value);
					list.Add(ps.SiteID, ps);
				}

				if (stat.PackOption == INPackageOption.Quantity)
				{
					List<INItemBoxEx> boxes = boxesByInventory[stat.InventoryID.Value];
					ps.Packages.AddRange(PackByQty(boxes, stat));
				}

				if (stat.PackOption == INPackageOption.Weight || stat.PackOption == INPackageOption.WeightAndVolume)
				{
					if (stat.InventoryID == ItemStats.Mixed)
					{
						Dictionary<string, INItemBoxEx> allboxes = new Dictionary<string, INItemBoxEx>();
						Dictionary<string, List<int>> boxItemLookup = new Dictionary<string, List<int>>();

						foreach (ItemInfo item in stat.Lines)
						{
							List<INItemBoxEx> boxes = boxesByInventory[item.InventoryID];
							foreach (INItemBoxEx box in boxes)
							{
								if (!allboxes.ContainsKey(box.BoxID))
								{
									allboxes.Add(box.BoxID, box);
								}

								if (!boxItemLookup.ContainsKey(box.BoxID))
								{
									boxItemLookup.Add(box.BoxID, new List<int>());
								}
								boxItemLookup[box.BoxID].Add(item.InventoryID);
							}
						}


						ps.Packages.AddRange(PackByWeightMixedItems(new List<INItemBoxEx>(allboxes.Values), new List<ItemInfo>(stat.Lines), boxesByInventory, boxItemLookup, stat.PackOption == INPackageOption.WeightAndVolume, stat.SiteID));
					}
					else
					{
						List<INItemBoxEx> boxes = boxesByInventory[stat.InventoryID.Value];
						ps.Packages.AddRange(PackByWeight(boxes, stat));
					}
				}
			}

			sw.Stop();
			Debug.Print("SOPackageEngine.Pack() in {0} millisec.", sw.ElapsedMilliseconds);
			return list.Values.ToList();
		}

		public virtual List<SOPackageInfoEx> PackByQty(List<INItemBoxEx> boxes, ItemStats stats)
		{
			List<SOPackageInfoEx> list = new List<SOPackageInfoEx>();

			List<BoxInfo> boxList = PackByQty(boxes, stats.BaseQty);

			foreach (BoxInfo box in boxList)
			{
				SOPackageInfoEx pack = new SOPackageInfoEx();
				pack.SiteID = stats.SiteID;
				pack.BoxID = box.Box.BoxID;
				pack.CarrierBox = box.Box.CarrierBox;
				pack.InventoryID = stats.InventoryID;
				pack.DeclaredValue = (stats.DeclaredValue / stats.BaseQty) * box.Value;
				pack.Weight = (stats.BaseWeight / stats.BaseQty) * box.Value + box.Box.BoxWeight.GetValueOrDefault();
				pack.Qty = box.Value;
				pack.Length = box.Box.Length;
				pack.Width = box.Box.Width;
				pack.Height = box.Box.Height;

				list.Add(pack);
			}


			return list;
		}

		public virtual List<SOPackageInfoEx> PackByWeight(List<INItemBoxEx> boxes, ItemStats stats)
		{
			List<SOPackageInfoEx> list = new List<SOPackageInfoEx>();

			List<BoxInfo> boxList = PackByWeight(boxes, stats.BaseWeight);

			foreach (BoxInfo box in boxList)
			{
				SOPackageInfoEx pack = new SOPackageInfoEx();
				pack.SiteID = stats.SiteID;
				pack.BoxID = box.Box.BoxID;
				pack.CarrierBox = box.Box.CarrierBox;
				pack.InventoryID = stats.InventoryID;
				pack.DeclaredValue = (stats.DeclaredValue / stats.BaseWeight) * box.Value;
				pack.Weight = box.Value + box.Box.BoxWeight.GetValueOrDefault();
				pack.Length = box.Box.Length;
				pack.Width = box.Box.Width;
				pack.Height = box.Box.Height;

				list.Add(pack);
			}



			return list;
		}

		public virtual List<SOPackageInfoEx> PackByWeightMixedItems(List<INItemBoxEx> boxes, List<ItemInfo> items, Dictionary<int, List<INItemBoxEx>> boxesByInventoryLookup, Dictionary<string, List<int>> boxItemsLookup, bool restrictByVolume, int? siteID)
		{
			// ������� c �������� (����� ���� � ����������, ����������� �������� ������)
			List<advancedINItemBoxEx> advancedboxes = new List<advancedINItemBoxEx>();

			// ���������� ������� ��� ������� ������
			foreach (KeyValuePair<int, List<INItemBoxEx>> keyValuePair in boxesByInventoryLookup)
			{
				keyValuePair.Value.Sort((x, y) => (-1 * decimal.Compare(x.MaxNetWeight, y.MaxNetWeight)));//DESC;
			}

			// ���������� ��� ������ �� ���� �����(���������) �������
			items.Sort((x, y) => (-1 * decimal.Compare(x.UnitWeight.GetValueOrDefault(), y.UnitWeight.GetValueOrDefault())));//DESC;

			// ���������� ��� ������� ��� ���� �������
			boxes.Sort((x, y) => (-1 * decimal.Compare(x.MaxNetWeight, y.MaxNetWeight)));//DESC;

			// ���������� ��� ������ ������� � ������ �������� (������) 
			if (restrictByVolume)
			{
				foreach (ItemInfo item in items)
				{
					// � ����� ������� ����� �� ������
					INItemBoxEx currentbox = boxesByInventoryLookup[item.InventoryID].ToArray()[0];
					decimal unpackedWeight = item.TotalWeight;
					decimal unpackedVolume = item.TotalVolume;

					if (item.UnitWeight > currentbox.MaxWeight || (item.UnitVolume > currentbox.MaxVolume && currentbox.MaxVolume > 0))
					{
						InventoryItem inv = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(graph, item.InventoryID);
						throw new PXException(Messages.NoBoxForItem, inv.InventoryCD.Trim());
					}

					// ����� ������� ����? 
					foreach (advancedINItemBoxEx advancedbox in advancedboxes)
					{
						foreach (INItemBoxEx allcurrentbox in boxesByInventoryLookup[item.InventoryID].ToArray())
						{
							if (advancedbox.BoxID == allcurrentbox.BoxID && unpackedWeight > 0)
							{
								if (advancedbox.EmptyBoxWeight >= item.UnitWeight && advancedbox.EmptyBoxVolume >= item.UnitVolume)
								{
									int numberofunitWeight;//qty that can fit in the box by weight
									int numberofunitVolume;//qty that can fit in the box by volume
									if (advancedbox.EmptyBoxWeight > unpackedWeight)
									{
										if (item.UnitWeight > 0)
											numberofunitWeight = Convert.ToInt32(Math.Floor(unpackedWeight/item.UnitWeight.Value));
										else
											numberofunitWeight = Convert.ToInt32(Math.Floor(item.Qty));
									}
									else
									{
										if (item.UnitWeight > 0)
											numberofunitWeight = Convert.ToInt32(Math.Floor(advancedbox.EmptyBoxWeight / item.UnitWeight.Value));
										else
											numberofunitWeight = Convert.ToInt32(Math.Floor(item.Qty));
									}

									if (advancedbox.EmptyBoxVolume > unpackedVolume)
									{
										if (item.UnitVolume > 0)
											numberofunitVolume = Convert.ToInt32(Math.Floor(unpackedVolume / item.UnitVolume.Value));
										else
											numberofunitVolume = Convert.ToInt32(Math.Floor(item.Qty));
									}
									else
									{
										if (item.UnitVolume > 0)
											numberofunitVolume = Convert.ToInt32(Math.Floor(advancedbox.EmptyBoxVolume / item.UnitVolume.Value));
										else
											numberofunitVolume = Convert.ToInt32(Math.Floor(item.Qty));
									}
									int numberofunit = numberofunitVolume < numberofunitWeight ? numberofunitVolume : numberofunitWeight;

									decimal actualweigth = numberofunit * item.UnitWeight.GetValueOrDefault();
									decimal actualvolume = numberofunit * item.UnitVolume.GetValueOrDefault();

									unpackedWeight = unpackedWeight - actualweigth;

									advancedbox.InvenoryList.Add(item.InventoryID);
									advancedbox.CurrentWeight = advancedbox.CurrentWeight + actualweigth;
									advancedbox.EmptyBoxWeight = advancedbox.MaxNetWeight - advancedbox.CurrentWeight;

									advancedbox.CurrentVolume = advancedbox.CurrentVolume + actualvolume;
									advancedbox.EmptyBoxVolume = advancedbox.MaxVolume.GetValueOrDefault() - advancedbox.CurrentVolume;
								}
							}
						}
					}

					if (unpackedWeight != 0)
					{
						// ������ � ����� ������ �����
						while (unpackedWeight > currentbox.MaxNetWeight)
						{
							int numberofunitWeight;
							int numberofunitVolume;


							if (item.UnitWeight > 0)
								numberofunitWeight = Convert.ToInt32(Math.Floor(currentbox.MaxNetWeight / item.UnitWeight.Value));
							else
								numberofunitWeight = Convert.ToInt32(Math.Floor(item.Qty));

							if (item.UnitVolume > 0 && currentbox.MaxVolume > 0)
								numberofunitVolume = Convert.ToInt32(Math.Floor(currentbox.MaxVolume.Value / item.UnitVolume.Value));
							else
								numberofunitVolume = Convert.ToInt32(Math.Floor(item.Qty));
							
							int numberofunit = numberofunitVolume < numberofunitWeight ? numberofunitVolume : numberofunitWeight;

							decimal actualweigth = numberofunit * item.UnitWeight.GetValueOrDefault();
							decimal actualvolume = numberofunit * item.UnitVolume.GetValueOrDefault();

							unpackedWeight = unpackedWeight - actualweigth;
							unpackedVolume = unpackedVolume - actualvolume;

							advancedINItemBoxEx advancedbox = new advancedINItemBoxEx();
							advancedbox.InvenoryList = new List<int>();
							advancedbox.BoxID = currentbox.BoxID;
							advancedbox.InvenoryList.Add(item.InventoryID);
							advancedbox.BoxWeight = currentbox.BoxWeight;
							advancedbox.MaxWeight = currentbox.MaxWeight;
							advancedbox.EmptyBoxWeight = currentbox.MaxNetWeight - actualweigth;
							advancedbox.EmptyBoxVolume = currentbox.MaxVolume.GetValueOrDefault() - actualvolume;
							advancedbox.CurrentWeight = actualweigth;
							advancedbox.CurrentVolume = actualvolume;
							advancedboxes.Add(advancedbox);
						}

						// ��������� �����
						while (unpackedWeight != 0)
						{
							int numberofunitWeight;
							int numberofunitVolume;

							if (currentbox.MaxNetWeight > unpackedWeight)
							{
								if (item.UnitWeight > 0)
									numberofunitWeight = Convert.ToInt32(Math.Floor(unpackedWeight / item.UnitWeight.Value));
								else
									numberofunitWeight = Convert.ToInt32(Math.Floor(item.Qty));
							}
							else
							{
								if (item.UnitWeight > 0)
									numberofunitWeight = Convert.ToInt32(Math.Floor(currentbox.MaxNetWeight / item.UnitWeight.Value));
								else
									numberofunitWeight = Convert.ToInt32(Math.Floor(item.Qty));
							}

							if (currentbox.MaxVolume > unpackedVolume)
							{
								if (item.UnitVolume > 0 )
									numberofunitVolume = Convert.ToInt32(Math.Floor(unpackedVolume / item.UnitVolume.Value));
								else
									numberofunitVolume = Convert.ToInt32(Math.Floor(item.Qty));
							}
							else
							{
								if (item.UnitVolume > 0 && currentbox.MaxVolume > 0)
									numberofunitVolume = Convert.ToInt32(Math.Floor(currentbox.MaxVolume.Value / item.UnitVolume.Value));
								else
									numberofunitVolume = Convert.ToInt32(Math.Floor(item.Qty));
							}

							int numberofunit = numberofunitVolume < numberofunitWeight ? numberofunitVolume : numberofunitWeight;

							decimal actualweigth = numberofunit * item.UnitWeight.GetValueOrDefault();
							decimal actualvolume = numberofunit * item.UnitVolume.GetValueOrDefault();

							unpackedWeight = unpackedWeight - actualweigth;
							unpackedVolume = unpackedVolume - actualvolume;

							advancedINItemBoxEx advancedbox1 = new advancedINItemBoxEx();
							advancedbox1.InvenoryList = new List<int>();
							advancedbox1.BoxID = currentbox.BoxID;
							advancedbox1.InvenoryList.Add(item.InventoryID);
							advancedbox1.BoxWeight = currentbox.BoxWeight;
							advancedbox1.MaxWeight = currentbox.MaxWeight;
							advancedbox1.EmptyBoxWeight = currentbox.MaxNetWeight - actualweigth;
							advancedbox1.EmptyBoxVolume = currentbox.MaxVolume.GetValueOrDefault() - actualvolume;
							advancedbox1.CurrentWeight = actualweigth;
							advancedbox1.CurrentVolume = actualvolume;
							advancedboxes.Add(advancedbox1);
						}
					}
				}

				// ������ ������. ��������� �� ������ �� ����� � ������� ��������
				foreach (advancedINItemBoxEx advancedbox in advancedboxes)
				{
					// ����� �� ���� �������� 
					foreach (INItemBoxEx box in boxes)
					{
						// ���� ���� ������� ������ ���� ����� (��� ������)
						bool availablesmallpackage = true;
						foreach (int itemlist in advancedbox.InvenoryList)
						{
							if (!boxItemsLookup[box.BoxID].Contains(itemlist))
								availablesmallpackage = false;
						}
						if (availablesmallpackage)
							// � �� ���� ��������
							if (advancedbox.CurrentWeight < box.MaxNetWeight)
								// � �� ������
								if (advancedbox.CurrentVolume < box.MaxVolume)
									advancedbox.BoxID = box.BoxID;
					}
				}
			}
			else
			{
				foreach (ItemInfo item in items)
				{
					// � ����� ������� ����� �� ������
					INItemBoxEx currentbox = boxesByInventoryLookup[item.InventoryID].ToArray()[0];
					decimal unpackedWeight = item.TotalWeight;
					// ����� ������� ����? 
					foreach (advancedINItemBoxEx advancedbox in advancedboxes)
					{
						foreach (INItemBoxEx allcurrentbox in boxesByInventoryLookup[item.InventoryID].ToArray())
						{
							if (advancedbox.BoxID == allcurrentbox.BoxID && unpackedWeight > 0)
							{
								if (advancedbox.EmptyBoxWeight >= item.UnitWeight)
								{
									int numberofunit;
									if (advancedbox.EmptyBoxWeight > unpackedWeight)
									{
										if (item.UnitWeight > 0)
											numberofunit = Convert.ToInt32(Math.Ceiling(unpackedWeight / item.UnitWeight.Value));
										else
											numberofunit = Convert.ToInt32(Math.Ceiling(item.Qty));
									}
									else
									{
										if (item.UnitWeight > 0)
											numberofunit = Convert.ToInt32(Math.Ceiling(advancedbox.EmptyBoxWeight / item.UnitWeight.Value));
										else
											numberofunit = Convert.ToInt32(Math.Ceiling(item.Qty));
									}

									decimal actualweigth = numberofunit * item.UnitWeight.GetValueOrDefault();
									unpackedWeight = unpackedWeight - actualweigth;

									advancedbox.InvenoryList.Add(item.InventoryID);
									advancedbox.CurrentWeight = advancedbox.CurrentWeight + actualweigth;
									advancedbox.EmptyBoxWeight = advancedbox.MaxNetWeight - advancedbox.CurrentWeight;
								}
							}
						}
					}

					if (unpackedWeight != 0)
					{
						// ������ � ����� ������ �����
						while (unpackedWeight >
							   currentbox.MaxNetWeight)
						{
							int numberofunit;
							if (item.UnitWeight > 0)
								numberofunit = Convert.ToInt32(Math.Ceiling(currentbox.MaxNetWeight / item.UnitWeight.Value));
							else
								numberofunit = Convert.ToInt32(Math.Ceiling(item.Qty));
							
							decimal actualweigth = numberofunit * item.UnitWeight.GetValueOrDefault();
							unpackedWeight = unpackedWeight - actualweigth;

							advancedINItemBoxEx advancedbox = new advancedINItemBoxEx();
							advancedbox.InvenoryList = new List<int>();
							advancedbox.BoxID = currentbox.BoxID;
							advancedbox.InvenoryList.Add(item.InventoryID);
							advancedbox.BoxWeight = currentbox.BoxWeight;
							advancedbox.MaxWeight = currentbox.MaxWeight;
							advancedbox.EmptyBoxWeight = currentbox.MaxNetWeight - actualweigth;
							advancedbox.CurrentWeight = actualweigth;
							advancedboxes.Add(advancedbox);
						}

						// � ��������� ����
						advancedINItemBoxEx advancedbox1 = new advancedINItemBoxEx();
						advancedbox1.InvenoryList = new List<int>();
						advancedbox1.BoxID = currentbox.BoxID;
						advancedbox1.InvenoryList.Add(item.InventoryID);
						advancedbox1.BoxWeight = currentbox.BoxWeight;
						advancedbox1.MaxWeight = currentbox.MaxWeight;
						advancedbox1.EmptyBoxWeight = currentbox.MaxNetWeight - unpackedWeight;
						advancedbox1.CurrentWeight = unpackedWeight;
						advancedboxes.Add(advancedbox1);
					}
				}
				// ������ ������. ��������� �� ������ �� ����� � ������� ��������
				foreach (advancedINItemBoxEx advancedbox in advancedboxes)
				{
					// ����� �� ���� �������� 
					foreach (INItemBoxEx box in boxes)
					{
						// ���� ���� ������� ������ ���� ����� (��� ������)
						bool availablesmallpackage = true;
						foreach (int itemlist in advancedbox.InvenoryList)
						{
							if (!boxItemsLookup[box.BoxID].Contains(itemlist))
								availablesmallpackage = false;
						}
						if (availablesmallpackage)
							// � �� ���� ��������
							if (advancedbox.CurrentWeight < box.MaxNetWeight)
								advancedbox.BoxID = box.BoxID;
					}
				}
			}

			List<SOPackageInfoEx> list = new List<SOPackageInfoEx>();
			foreach (advancedINItemBoxEx advancedbox in advancedboxes)
			{
				SOPackageInfoEx temp = new SOPackageInfoEx();
				temp.BoxID = advancedbox.BoxID;
				temp.SiteID = siteID;
				foreach (INItemBoxEx box in boxes)
				{
					if (box.BoxID == temp.BoxID)
						temp.Weight = advancedbox.CurrentWeight + box.BoxWeight;
				}
				list.Add(temp);
			}
			return list;
		}
		
		public virtual List<BoxInfo> PackByQty(List<INItemBoxEx> boxes, decimal baseQty)
		{
			boxes.Sort((INItemBoxEx x, INItemBoxEx y) => decimal.Compare(x.BaseQty.GetValueOrDefault(), y.BaseQty.GetValueOrDefault()));

			List<BoxInfo> list = new List<BoxInfo>();
			if (baseQty > 0 && boxes.Count > 0)
			{
				INItemBoxEx box = GetBoxThatCanFitQty(boxes, baseQty);
				if (box != null)
				{
					BoxInfo p = new BoxInfo();
					p.Box = box;
					p.Value = baseQty;
					list.Add(p);
				}
				else
				{
					//Distribute qty among Biggest boxes available: 
					INItemBoxEx biggestBox = boxes[boxes.Count - 1];
					if (biggestBox.BaseQty.Value > 0)
					{
						int numberOfMaxBoxes = (int) Math.Floor(baseQty/biggestBox.BaseQty.Value);

						for (int i = 0; i < numberOfMaxBoxes; i++)
						{
							BoxInfo p = new BoxInfo();
							p.Box = biggestBox;
							p.Value = biggestBox.BaseQty.Value;
							list.Add(p);

							baseQty -= biggestBox.BaseQty.Value;
						}

						//remainder
						list.AddRange(PackByQty(boxes, baseQty));
					}
				}
			}

			return list;
		}

		public virtual List<BoxInfo> PackByWeight(List<INItemBoxEx> boxes, decimal baseWeight)
		{
			boxes.Sort((INItemBoxEx x, INItemBoxEx y) => decimal.Compare(x.MaxWeight.GetValueOrDefault(), y.MaxWeight.GetValueOrDefault()));

			List<BoxInfo> list = new List<BoxInfo>();
			if (baseWeight > 0 && boxes.Count > 0)
			{
				INItemBoxEx box = GetBoxThatCanFitWeight(boxes, baseWeight);
				if (box != null)
				{
					BoxInfo p = new BoxInfo();
					p.Box = box;
					p.Value = baseWeight;
					list.Add(p);
				}
				else
				{
					//Distribute qty among Biggest boxes available: 
					INItemBoxEx biggestBox = boxes[boxes.Count - 1];
					int numberOfMaxBoxes = (int)Math.Floor(baseWeight / (biggestBox.MaxWeight.Value - biggestBox.BoxWeight.GetValueOrDefault()));

					for (int i = 0; i < numberOfMaxBoxes; i++)
					{
						BoxInfo p = new BoxInfo();
						p.Box = biggestBox;
						p.Value = biggestBox.MaxWeight.Value;
						list.Add(p);

						baseWeight -= (biggestBox.MaxWeight.Value - biggestBox.BoxWeight.GetValueOrDefault());
					}

					//remainder
					list.AddRange(PackByWeight(boxes, baseWeight));
				}
			}

			return list;
		}

		public virtual List<INItemBoxEx> GetBoxesByInventoryID(int inventoryID, string carrierID)
		{
			PXSelectBase<INItemBoxEx> select;
			if (string.IsNullOrEmpty(carrierID))
			{
				select = new PXSelect<INItemBoxEx,
					Where<INItemBoxEx.inventoryID, Equal<Required<INItemBoxEx.inventoryID>>>>(graph);
			}
			else
			{
				select = new PXSelectJoin<INItemBoxEx,
					InnerJoin<CarrierPackage, On<INItemBoxEx.boxID, Equal<CarrierPackage.boxID>>>,
					Where<INItemBoxEx.inventoryID, Equal<Required<INItemBox.inventoryID>>,
					And<CarrierPackage.carrierID, Equal<Required<CarrierPackage.carrierID>>>>>(graph);
			}

			List<INItemBoxEx> list = new List<INItemBoxEx>();
			foreach (INItemBoxEx box in select.Select(inventoryID, carrierID))
			{
				list.Add(box);
			}

			if (list.Count == 0 )
			{
				if (string.IsNullOrEmpty(carrierID))
				{
					//there are no explicit boxes setup on the InventoryItem. This means that all system boxes are applicable:
					List<CSBox> allboxes = GetBoxesByCarrierID(carrierID);
					foreach (CSBox csBox in allboxes)
					{
						INItemBoxEx box = new INItemBoxEx();
						box.BoxID = csBox.BoxID;
						box.BaseQty = null;
						box.BoxWeight = csBox.BoxWeight;
						box.CarrierBox = csBox.CarrierBox;
						box.Description = csBox.Description;
						box.InventoryID = inventoryID;
						box.MaxQty = null;
						box.MaxVolume = csBox.MaxVolume;
						box.MaxWeight = csBox.MaxWeight;
						box.Qty = null;
						box.UOM = null;
						list.Add(box);
					}
				}
				else
				{
					//There is no common box between boxes defined for the item and boxess defined for the carrier.
					throw new PXException(Messages.BoxesNotDefined);
				}
			}
			
			return list;
		}

		public virtual List<CSBox> GetBoxesByCarrierID(string carrierID)
		{
			List<CSBox> list = new List<CSBox>();
			if (string.IsNullOrEmpty(carrierID))
			{
				foreach (CSBox box in PXSelect<CSBox, Where<CSBox.maxWeight, Greater<decimal0>>>.Select(graph))
				{
					list.Add(box);
				}
			}
			else
			{
				PXSelectBase<CSBox> select = new PXSelectJoin<CSBox,
					InnerJoin<CarrierPackage, On<CSBox.boxID, Equal<CarrierPackage.boxID>>>,
					Where<CarrierPackage.carrierID, Equal<Required<CarrierPackage.carrierID>>,
					And<CSBox.maxWeight, Greater<decimal0>>>, OrderBy<Asc<CSBox.maxWeight>>>(graph);

				foreach (CSBox box in select.Select(carrierID))
				{
					list.Add(box);
				}
			}

			return list;
		}

		protected INItemBoxEx GetBoxThatCanFitQty(List<INItemBoxEx> boxes, decimal baseQty)
		{
			for (int i = 0; i < boxes.Count; i++)
			{
				if (boxes[i].BaseQty >= baseQty)
					return boxes[i];
			}

			return null;
		}

		protected INItemBoxEx GetBoxThatCanFitWeight(List<INItemBoxEx> boxes, decimal baseWeight)
		{
			for (int i = 0; i < boxes.Count; i++)
			{
				if (boxes[i].MaxWeight >= baseWeight + boxes[i].BoxWeight.GetValueOrDefault())
					return boxes[i];
			}

			return null;
		}

		
		public class BoxInfo
		{
			public INItemBoxEx Box { get; set; }
			public decimal Value { get; set; }
		}

		public class ItemStats
		{
			public const int Mixed = 0;

			public int? SiteID { get; set; }
			public int? InventoryID { get; set; }
			public string PackOption { get; set; }
			public string Operation { get; set; }
			public decimal BaseQty { get; set; }
			public decimal BaseWeight { get; set; }
			public decimal DeclaredValue { get; set; }

			private Dictionary<int, ItemInfo> items = new Dictionary<int, ItemInfo>();

			public ItemStats()
			{}

			public void AddLine(InventoryItem item, decimal? baseQty)
			{
				if (items.ContainsKey(item.InventoryID.Value))
				{
					items[item.InventoryID.Value].Qty += baseQty.GetValueOrDefault();
				}
				else
				{
					ItemInfo line = new ItemInfo(item.InventoryID.Value, item.BaseWeight, item.BaseVolume, baseQty.GetValueOrDefault());
					items.Add(line.InventoryID, line);
				}

			}

			public ICollection<ItemInfo> Lines
			{
				get { return items.Values; }
			}
		}
	
		public class OrderInfo
		{
			public string CarrierID { get; set; }

			private Dictionary<int, ItemInfo> items = new Dictionary<int, ItemInfo>();
			public List<ItemStats> Stats { get; private set; }

			public OrderInfo(string carrierID)
			{
				Stats = new List<ItemStats>();
				this.CarrierID = carrierID;
			}

			public void AddLine(InventoryItem item, decimal? baseQty)
			{
				if (items.ContainsKey(item.InventoryID.Value))
				{
					items[item.InventoryID.Value].Qty += baseQty.GetValueOrDefault();
				}
				else
				{
					ItemInfo line = new ItemInfo(item.InventoryID.Value, item.BaseWeight, item.BaseVolume, baseQty.GetValueOrDefault());
					items.Add(line.InventoryID, line);
				}

			}

			public ICollection<ItemInfo> Lines
			{
				get { return items.Values; }
			}
		}

		public class ItemInfo
		{
			public int InventoryID { get; private set; }
			public decimal? UnitWeight { get; private set; }
			public decimal? UnitVolume { get; private set; }
			public decimal TotalWeight 
			{
				get { return UnitWeight.GetValueOrDefault() * Qty; }
			}
			public decimal TotalVolume
			{
				get { return UnitVolume.GetValueOrDefault() * Qty; }
			}
			public decimal Qty { get; set; }

			public ItemInfo(int inventoryID, decimal? unitWeight, decimal? unitVolume, decimal qty)
			{
				this.InventoryID = inventoryID;
				this.UnitWeight = unitWeight;
				this.UnitVolume = unitVolume;
				this.Qty = qty;
			}
		}

		public class PackSet
		{
			public int SiteID { get; private set; }
			public List<SOPackageInfoEx> Packages { get; private set; }

			public PackSet(int siteID)
			{
				this.SiteID = siteID;
				this.Packages = new List<SOPackageInfoEx>();
			}
		}

		private class advancedINItemBoxEx : INItemBoxEx
		{
			/// <summary>
			/// Amount of weight that can be added to this box. Empty space.
			/// </summary>
			public Decimal EmptyBoxWeight { get; set; }

			/// <summary>
			/// Amount of volume that can be added to this box. Empty space.
			/// </summary>
			public Decimal EmptyBoxVolume { get; set; }

			public Decimal CurrentWeight { get; set; }
			public Decimal CurrentVolume { get; set; }
			public List<Int32> InvenoryList { get; set; }
		}
	}
}
