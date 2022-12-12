using System.Collections;
using System.Data;

namespace DataTableTransformer;

public class DataTableTransformer
{
    public List<RawRow> Transform(DataTable dataTable, QueryMetadata query)
    {
        var result = new List<RawRow>();

        foreach (DataRow row in dataTable.Rows)
        {
            RawRow rawRow = new()
            {
                Index = result.Count,
                DataRow = row,
                Table = query.Table,
                // FIXME: First (0) column maybe not `Identity`
                Id = (int)row[query.Table.Columns[0].Alias],
                Data = Activator.CreateInstance(query.Table.Type)!,
                Joins = new(),
            };

            foreach (var column in query.Table.Columns)
            {
                column.PropertyInfo.SetValue(rawRow.Data, row[column.Alias]);
            }

            foreach (var include in query.Includes)
            {
                rawRow.Joins.AddRange(TransformToRawRowJoinedTable(row, include));
            }

            result.Add(rawRow);
        }

        return result;
    }

    private List<RawRowJoinedTable> TransformToRawRowJoinedTable(DataRow row, IncludeMetadata include)
    {
        List<RawRowJoinedTable> list = new();

        RawRowJoinedTable joinedTable = new()
        {
            IncludeMetadata = include,
            Data = Activator.CreateInstance(include.JoinedTable.Type)!,
            Id = -1,
        };

        foreach (var column in include.JoinedTable.Columns)
        {
            column.PropertyInfo.SetValue(joinedTable.Data, row[column.Alias]);
        }

        // FIXME: First column maybe not primary column
        joinedTable.Id = (int)row[include.JoinedTable.Columns[0].Alias];

        list.Add(joinedTable);

        foreach (var subInclude in include.SubIncludes)
        {
            // FIXME: Maybe we can also make nested list instead float
            //        This may make easy to create models
            list.AddRange(TransformToRawRowJoinedTable(row, subInclude));
        }

        return list;
    }

    public List<T> TransformToList<T>(List<RawRow> rows, QueryMetadata query)
    {
        List<T> list = new();

        foreach (var group in rows.GroupBy(x => x.Id))
        {
            var firstRow = group.First();
            var mainData = firstRow.Data;
            var subRawRows = group.ToList();

            foreach (var include in query.Includes)
            {
                SetObjectsVisitor(mainData, include, subRawRows);
            }

            list.Add((T)mainData);
        }

        return list;
    }

    public List<RawRowJoinedTableInfo> GetRawRowJoinedTable(List<RawRow> rows, IncludeMetadata includeMetadata)
    {
        var dictionary = new Dictionary<int, RawRowJoinedTableInfo>();

        foreach (var row in rows)
        {
            var rawRowJoinedTable = row.Joins
                .First(x => x.IncludeMetadata.JoinedTable.Alias == includeMetadata.JoinedTable.Alias);

            if (!dictionary.ContainsKey(rawRowJoinedTable.Id))
            {
                var info = new RawRowJoinedTableInfo()
                {
                    TableData = rawRowJoinedTable,
                    SubRawRows = new() { row },
                };

                dictionary[rawRowJoinedTable.Id] = info;
            }
            else
            {
                dictionary[rawRowJoinedTable.Id].SubRawRows.Add(row);
            }
        }

        return dictionary.Values.ToList();
    }

    public void SetObjectsVisitor(object parent, IncludeMetadata includeMetadata, List<RawRow> rows)
    {
        var joinedRows = GetRawRowJoinedTable(rows, includeMetadata);

        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(includeMetadata.JoinedTable.Type);

        var list = (IList)Activator.CreateInstance(constructedListType)!;

        foreach (var joinedRow in joinedRows)
        {
            var child = joinedRow.TableData.Data;

            foreach (var subInclude in includeMetadata.SubIncludes)
            {
                SetObjectsVisitor(child, subInclude, joinedRow.SubRawRows);
            }

            list.Add(child);
        }

        object? value = includeMetadata.IsOneToOne ? list.Cast<object>().FirstOrDefault() : list;

        // FIXME: Property type maybe not `List<>`
        includeMetadata.ParentRelationObjectPropertyInfo.SetValue(parent, value);
    }
}

public class RawRow
{
    public int Index { get; set; }
    public DataRow DataRow { get; set; } = null!;
    public TableMetadata Table { get; set; } = null!;
    public int Id { get; set; }
    public object Data { get; set; } = null!;
    public List<RawRowJoinedTable> Joins { get; set; } = null!;
}

public class RawRowJoinedTableInfo
{
    public RawRowJoinedTable TableData { get; set; } = null!;
    public List<RawRow> SubRawRows { get; set; } = null!;
}

public class RawRowJoinedTable
{
    public IncludeMetadata IncludeMetadata { get; set; } = null!;
    public int Id { get; set; }
    public object Data { get; set; } = null!;
}
