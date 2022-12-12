using System.Data;
using DataTableTransformer;

namespace DataTableTransformerTests;

public class DataTableTransformerTests
{
    [Fact]
    public void IsItWorking()
    {
        var fooTableMetadata = Foo.GetTableMetadata("T1_");
        var barTableMetadata = Bar.GetTableMetadata("T2_");
        var quxTableMetadata = Qux.GetTableMetadata("T3_");
        var quuxTableMetadata = Quux.GetTableMetadata("T4_");

        QueryMetadata queryMetadata = new()
        {
            Table = fooTableMetadata,
            Columns = fooTableMetadata.Columns,
            Includes = new(),
        };

        queryMetadata.Includes.Add(new()
        {
            ParentTable = fooTableMetadata,
            ParentRelationObjectPropertyInfo = fooTableMetadata.Type.GetProperty("Bars")!,
            JoinedTable = barTableMetadata,
            JoinedColumn = barTableMetadata.Columns[1],
            IsOneToOne = false,
            IsOneToMany = true,
            SubIncludes = new()
            {
                new()
                {
                    ParentTable = barTableMetadata,
                    ParentRelationObjectPropertyInfo = barTableMetadata.Type.GetProperty("Qux")!,
                    JoinedTable = quxTableMetadata,
                    JoinedColumn = quxTableMetadata.Columns[1],
                    IsOneToOne = true,
                    IsOneToMany = false,
                    SubIncludes = new(),
                },
            },
        });

        queryMetadata.Includes.Add(new()
        {
            ParentTable = fooTableMetadata,
            ParentRelationObjectPropertyInfo = fooTableMetadata.Type.GetProperty("Quuxs")!,
            JoinedTable = quuxTableMetadata,
            JoinedColumn = quuxTableMetadata.Columns[1],
            IsOneToOne = false,
            IsOneToMany = true,
            SubIncludes = new(),
        });

        DataTable dataTable = new();
        dataTable.Columns.Add("T1_Id", typeof(int));
        dataTable.Columns.Add("T1_Name", typeof(string));
        dataTable.Columns.Add("T2_Id", typeof(int));
        dataTable.Columns.Add("T2_FooId", typeof(int));
        dataTable.Columns.Add("T2_QuxId", typeof(int));
        dataTable.Columns.Add("T2_Name", typeof(string));
        dataTable.Columns.Add("T3_Id", typeof(int));
        dataTable.Columns.Add("T3_Name", typeof(string));
        dataTable.Columns.Add("T4_Id", typeof(int));
        dataTable.Columns.Add("T4_BarId", typeof(int));
        dataTable.Columns.Add("T4_Name", typeof(string));

        var row1 = dataTable.NewRow();
        row1["T1_Id"] = 1;
        row1["T1_Name"] = "T1_Name1";
        row1["T2_Id"] = 1;
        row1["T2_FooId"] = 1;
        row1["T2_QuxId"] = 1;
        row1["T2_Name"] = "T2_Name1";
        row1["T3_Id"] = 1;
        row1["T3_Name"] = "T3_Name1";
        row1["T4_Id"] = 1;
        row1["T4_BarId"] = 1;
        row1["T4_Name"] = "T4_Name1";
        dataTable.Rows.Add(row1);

        var row2 = dataTable.NewRow();
        row2["T1_Id"] = 1;
        row2["T1_Name"] = "T1_Name1";
        row2["T2_Id"] = 2;
        row2["T2_FooId"] = 1;
        row2["T2_QuxId"] = 1;
        row2["T2_Name"] = "T2_Name2";
        row2["T3_Id"] = 2;
        row2["T3_Name"] = "T3_Name2";
        row2["T4_Id"] = 2;
        row2["T4_BarId"] = 2;
        row2["T4_Name"] = "T4_Name2";
        dataTable.Rows.Add(row2);

        var row3 = dataTable.NewRow();
        row3["T1_Id"] = 2;
        row3["T1_Name"] = "T1_Name2";
        row3["T2_Id"] = 1;
        row3["T2_FooId"] = 1;
        row3["T2_QuxId"] = 1;
        row3["T2_Name"] = "T2_Name1";
        row3["T3_Id"] = 1;
        row3["T3_Name"] = "T3_Name1";
        row3["T4_Id"] = 1;
        row3["T4_BarId"] = 1;
        row3["T4_Name"] = "T4_Name1";
        dataTable.Rows.Add(row3);

        var row4 = dataTable.NewRow();
        row4["T1_Id"] = 2;
        row4["T1_Name"] = "T1_Name2";
        row4["T2_Id"] = 2;
        row4["T2_FooId"] = 2;
        row4["T2_QuxId"] = 1;
        row4["T2_Name"] = "T2_Name2";
        row4["T3_Id"] = 1;
        row4["T3_Name"] = "T3_Name1";
        row4["T4_Id"] = 1;
        row4["T4_BarId"] = 1;
        row4["T4_Name"] = "T4_Name1";
        dataTable.Rows.Add(row4);

        var row5 = dataTable.NewRow();
        row5["T1_Id"] = 2;
        row5["T1_Name"] = "T1_Name2";
        row5["T2_Id"] = 2;
        row5["T2_FooId"] = 2;
        row5["T2_QuxId"] = 1;
        row5["T2_Name"] = "T2_Name2";
        row5["T3_Id"] = 1;
        row5["T3_Name"] = "T3_Name1";
        row5["T4_Id"] = 1;
        row5["T4_BarId"] = 1;
        row5["T4_Name"] = "T4_Name1";
        dataTable.Rows.Add(row5);

        var transformer = new DataTableTransformer.DataTableTransformer();
        var rawRows = transformer.Transform(dataTable, queryMetadata);
        var result = transformer.TransformToList<Foo>(rawRows, queryMetadata);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result[0].Bars.Count);
        Assert.Equal(1, result[0].Bars[0].Id);
        Assert.Equal(2, result[0].Bars[1].Id);
        Assert.Equal("T2_Name1", result[0].Bars[0].Name);
        Assert.Equal("T2_Name2", result[0].Bars[1].Name);
        Assert.NotNull(result[0].Bars[0].Qux);
        Assert.NotNull(result[0].Bars[1].Qux);
        Assert.Equal(1, result[0].Bars[0].Qux.Id);
        Assert.Equal(2, result[0].Bars[1].Qux.Id);
        Assert.Equal("T3_Name1", result[0].Bars[0].Qux.Name);
        Assert.Equal("T3_Name2", result[0].Bars[1].Qux.Name);
    }

    private class Foo
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public List<Bar> Bars { get; set; } = null!;
        public List<Quux> Quuxs { get; set; } = null!;

        public static TableMetadata GetTableMetadata(string prefix)
        {
            TableMetadata tableMetadata = new()
            {
                TableName = nameof(Foo),
                Alias = prefix + nameof(Foo),
                Type = typeof(Foo),
                Columns = new(),
            };

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(Id))!,
                ColumnType = ColumnType.Int,
                ColumnName = nameof(Id),
                Alias = prefix + nameof(Id),
            });

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(Name))!,
                ColumnType = ColumnType.Varchar,
                ColumnName = nameof(Name),
                Alias = prefix + nameof(Name),
            });

            return tableMetadata;
        }
    }

    private class Bar
    {
        public int Id { get; set; }
        public int FooId { get; set; }
        public int QuxId { get; set; }
        public string Name { get; set; } = null!;

        public Qux Qux { get; set; } = null!;

        public static TableMetadata GetTableMetadata(string prefix)
        {
            TableMetadata tableMetadata = new()
            {
                TableName = nameof(Bar),
                Alias = prefix + nameof(Bar),
                Type = typeof(Bar),
                Columns = new(),
            };

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(Id))!,
                ColumnType = ColumnType.Int,
                ColumnName = nameof(Id),
                Alias = prefix + nameof(Id),
            });

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(FooId))!,
                ColumnType = ColumnType.Int,
                ColumnName = nameof(FooId),
                Alias = prefix + nameof(FooId),
            });

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(QuxId))!,
                ColumnType = ColumnType.Int,
                ColumnName = nameof(QuxId),
                Alias = prefix + nameof(QuxId),
            });

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(Name))!,
                ColumnType = ColumnType.Varchar,
                ColumnName = nameof(Name),
                Alias = prefix + nameof(Name),
            });

            return tableMetadata;
        }
    }

    private class Qux
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public static TableMetadata GetTableMetadata(string prefix)
        {
            TableMetadata tableMetadata = new()
            {
                TableName = nameof(Qux),
                Alias = prefix + nameof(Qux),
                Type = typeof(Qux),
                Columns = new(),
            };

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(Id))!,
                ColumnType = ColumnType.Int,
                ColumnName = nameof(Id),
                Alias = prefix + nameof(Id),
            });

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(Name))!,
                ColumnType = ColumnType.Varchar,
                ColumnName = nameof(Name),
                Alias = prefix + nameof(Name),
            });

            return tableMetadata;
        }
    }

    private class Quux
    {
        public int Id { get; set; }
        public int BarId { get; set; }
        public string Name { get; set; } = null!;

        public static TableMetadata GetTableMetadata(string prefix)
        {
            TableMetadata tableMetadata = new()
            {
                TableName = nameof(Quux),
                Alias = prefix + nameof(Quux),
                Type = typeof(Quux),
                Columns = new(),
            };

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(Id))!,
                ColumnType = ColumnType.Int,
                ColumnName = nameof(Id),
                Alias = prefix + nameof(Id),
            });

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(BarId))!,
                ColumnType = ColumnType.Int,
                ColumnName = nameof(BarId),
                Alias = prefix + nameof(BarId),
            });

            tableMetadata.Columns.Add(new()
            {
                Table = tableMetadata,
                PropertyInfo = tableMetadata.Type.GetProperty(nameof(Name))!,
                ColumnType = ColumnType.Varchar,
                ColumnName = nameof(Name),
                Alias = prefix + nameof(Name),
            });

            return tableMetadata;
        }
    }
}