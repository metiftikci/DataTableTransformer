# Data Table Transformer

## Goal

```csharp
class Invoice
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; }

    public List<InvoiceDetail> Details { get; set; }
}

class InvoiceDetail
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int ItemId { get; set; }
    public double Price { get; set; }

    public Item Item { get; set; }
}

class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

```sql
SELECT T1.Id        AS T1_Id
      ,T1.InvoiceNo AS T1_InvoiceNo
      ,T2.Id        AS T2_Id
      ,T2.ItemId    AS T2_ItemId
      ,T2.Price     AS T2_Price
      ,T3.Id        AS T3_Id
      ,T3.Name      AS T3_Name
FROM Invoice AS T1
LEFT JOIN InvoiceDetails T2 ON T2.InvoiceId = T1.Id
LEFT JOIN Item T3 ON T3.Id = T2.ItemId
```

| T1_Id | T1_InvoiceNo | T2_Id | T2_ItemId | T2_Price | T3_Id | T3_Name |
|-------|--------------|-------|-----------|---------:|-------|---------|
| 1     | 000000000001 | 1     | 1         |       10 | 1     | Item1   |
| 1     | 000000000001 | 2     | 2         |       20 | 2     | Item2   |
| 2     | 000000000002 | 3     | 3         |       30 | 3     | Item3   |
| 2     | 000000000002 | 4     | 4         |       40 | 4     | Item4   |

```csharp
var transformer = new DataTableTransformer();

List<Invoice> invoices = transformer.Transfor<Invoice>(dataTable, queryInfo);

Console.WriteLine(invoices.Count); // 2
Console.WriteLine(invoices[0].Details.Count); // 2
Console.WriteLine(invoices[1].Details[1].Item.Name); // Item4
```
