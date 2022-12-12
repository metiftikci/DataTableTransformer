using System.Reflection;

namespace DataTableTransformer;

public class QueryMetadata
{
    public List<ColumnMetadata> Columns { get; set; } = null!;
    public TableMetadata Table { get; set; } = null!;
    public List<IncludeMetadata> Includes { get; set; } = null!;
}

public class TableMetadata
{
    public string TableName { get; set; } = null!;
    public string Alias { get; set; } = null!;
    public List<ColumnMetadata> Columns { get; set; } = null!;
    public ColumnMetadata PrimaryKeyColumn { get; set; } = null!;
    public Type Type { get; set; } = null!;
}

public class ColumnMetadata
{
    public string ColumnName { get; set; } = null!;
    public string Alias { get; set; } = null!;
    public bool IsPrimaryKey { get; set; }
    public bool IsNullable { get; set; }
    public ColumnType ColumnType { get; set; }
    public int MaxLength { get; set; }
    public TableMetadata Table { get; set; } = null!;
    public PropertyInfo PropertyInfo { get; set; } = null!;
}

public enum ColumnType
{
    Boolean,
    Short,
    Int,
    Long,
    Float,
    Varchar,
    DataTime,
}

public class IncludeMetadata
{
    public TableMetadata ParentTable { get; set; } = null!;
    public PropertyInfo ParentRelationObjectPropertyInfo { get; set; } = null!;
    public TableMetadata JoinedTable { get; set; } = null!;
    public ColumnMetadata JoinedColumn { get; set; } = null!;
    public bool IsOneToOne { get; set; }
    public bool IsOneToMany { get; set; }
    public List<IncludeMetadata> SubIncludes { get; set; } = null!;
}

/*

T1: Customers
T2: Orders        1-*
T3: Order Details 1-*
T4: Items         1-1

DataTable Rows:
T1_Id, T1_Name, T1_Surname, T2_Id, T2_No,  T2_Date, T2_Total, T3_Id, T3_ItemId, T3_Amount, T4_Id, T4_Name
    1     Emin    TIFTIKCI      1    001  05.12.22       100      1          1          1      1    Item1
    1     Emin    TIFTIKCI      1    001  05.12.22       100      2          2          1      2    Item2
    1     Emin    TIFTIKCI      2    002  05.12.22       100      3          1          1      1    Item1
    1     Emin    TIFTIKCI      2    002  05.12.22       100      4          2          1      2    Item2

    2       ME    TIFTIKCI      3    003  05.12.22       100      5          1          1      1    Item1
    2       ME    TIFTIKCI      3    003  05.12.22       100      6          2          1      2    Item2
    2       ME    TIFTIKCI      4    004  05.12.22       100      7          1          1      1    Item1
    2       ME    TIFTIKCI      4    004  05.12.22       100      8          2          1      2    Item2


Customer , Order , Order Details , Item

Customers
  - Address
  - Orders
    - Order Details
      - Item

Convert this to model
*/
