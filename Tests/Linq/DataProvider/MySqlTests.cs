﻿extern alias MySqlData;
extern alias MySqlConnector;

using System;
using System.Data.Linq;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.SchemaProvider;
using LinqToDB.Tools;

using NUnit.Framework;

using MySqlDataDateTime = MySqlData::MySql.Data.Types.MySqlDateTime;
using MySqlDataDecimal  = MySqlData::MySql.Data.Types.MySqlDecimal;
using MySqlDataGeometry = MySqlData::MySql.Data.Types.MySqlGeometry;

using MySqlConnectorDateTime = MySqlConnector::MySql.Data.Types.MySqlDateTime;

namespace Tests.DataProvider
{
	using Model;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;

	[TestFixture]
	public class MySqlTests : DataProviderTestBase
	{
		[Test]
		public void TestParameters([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<string>("SELECT @p",        new { p =  1  }), Is.EqualTo("1"));
				Assert.That(conn.Execute<string>("SELECT @p",        new { p = "1" }), Is.EqualTo("1"));
				Assert.That(conn.Execute<int>   ("SELECT @p",        new { p =  new DataParameter { Value = 1   } }), Is.EqualTo(1));
				Assert.That(conn.Execute<string>("SELECT @p1",       new { p1 = new DataParameter { Value = "1" } }), Is.EqualTo("1"));
				Assert.That(conn.Execute<int>   ("SELECT @p1 + ?p2", new { p1 = 2, p2 = 3 }), Is.EqualTo(5));
				Assert.That(conn.Execute<int>   ("SELECT @p2 + ?p1", new { p2 = 2, p1 = 3 }), Is.EqualTo(5));
			}
		}

		[Test]
		public void TestDataTypes([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(TestType<long?>						(conn, "bigintDataType",    DataType.Int64),               Is.EqualTo(1000000));
				Assert.That(TestType<short?>					(conn, "smallintDataType",  DataType.Int16),               Is.EqualTo(25555));
				Assert.That(TestType<sbyte?>					(conn, "tinyintDataType",   DataType.SByte),               Is.EqualTo(111));
				Assert.That(TestType<int?>						(conn, "mediumintDataType", DataType.Int32),               Is.EqualTo(5555));
				Assert.That(TestType<int?>						(conn, "intDataType",       DataType.Int32),               Is.EqualTo(7777777));
				Assert.That(TestType<decimal?>					(conn, "numericDataType",   DataType.Decimal),             Is.EqualTo(9999999m));
				Assert.That(TestType<decimal?>					(conn, "decimalDataType",   DataType.Decimal),             Is.EqualTo(8888888m));
				Assert.That(TestType<double?>					(conn, "doubleDataType",    DataType.Double),              Is.EqualTo(20.31d));
				Assert.That(TestType<float?>					(conn, "floatDataType",     DataType.Single),              Is.EqualTo(16.0f));
				Assert.That(TestType<DateTime?>					(conn, "dateDataType",      DataType.Date),                Is.EqualTo(new DateTime(2012, 12, 12)));
				Assert.That(TestType<DateTime?>					(conn, "datetimeDataType",  DataType.DateTime),            Is.EqualTo(new DateTime(2012, 12, 12, 12, 12, 12)));
				Assert.That(TestType<DateTime?>					(conn, "datetimeDataType",  DataType.DateTime2),           Is.EqualTo(new DateTime(2012, 12, 12, 12, 12, 12)));
				Assert.That(TestType<DateTime?>					(conn, "timestampDataType", DataType.Timestamp),           Is.EqualTo(new DateTime(2012, 12, 12, 12, 12, 12)));
				Assert.That(TestType<TimeSpan?>					(conn, "timeDataType",      DataType.Time),                Is.EqualTo(new TimeSpan(12, 12, 12)));
				Assert.That(TestType<int?>						(conn, "yearDataType",      DataType.Int32),               Is.EqualTo(1998));
				Assert.That(TestType<int?>						(conn, "year2DataType",     DataType.Int32),               Is.EqualTo(context == TestProvName.MySql57 || context == ProviderName.MySqlConnector ? 1997 : 97));
				Assert.That(TestType<int?>						(conn, "year4DataType",     DataType.Int32),               Is.EqualTo(2012));

				Assert.That(TestType<char?>						(conn, "charDataType",      DataType.Char),                Is.EqualTo('1'));
				Assert.That(TestType<string>					(conn, "charDataType",      DataType.Char),                Is.EqualTo("1"));
				Assert.That(TestType<string>					(conn, "charDataType",      DataType.NChar),               Is.EqualTo("1"));
				Assert.That(TestType<string>					(conn, "varcharDataType",   DataType.VarChar),             Is.EqualTo("234"));
				Assert.That(TestType<string>					(conn, "varcharDataType",   DataType.NVarChar),            Is.EqualTo("234"));
				Assert.That(TestType<string>					(conn, "textDataType",      DataType.Text),                Is.EqualTo("567"));

				Assert.That(TestType<byte[]>					(conn, "binaryDataType",    DataType.Binary),              Is.EqualTo(new byte[] {  97,  98,  99 }));
				Assert.That(TestType<byte[]>					(conn, "binaryDataType",    DataType.VarBinary),           Is.EqualTo(new byte[] {  97,  98,  99 }));
				Assert.That(TestType<byte[]>					(conn, "varbinaryDataType", DataType.Binary),              Is.EqualTo(new byte[] {  99, 100, 101 }));
				Assert.That(TestType<byte[]>					(conn, "varbinaryDataType", DataType.VarBinary),           Is.EqualTo(new byte[] {  99, 100, 101 }));
				Assert.That(TestType<Binary>					(conn, "varbinaryDataType", DataType.VarBinary).ToArray(), Is.EqualTo(new byte[] {  99, 100, 101 }));
				Assert.That(TestType<byte[]>					(conn, "blobDataType",      DataType.Binary),              Is.EqualTo(new byte[] { 100, 101, 102 }));
				Assert.That(TestType<byte[]>					(conn, "blobDataType",      DataType.VarBinary),           Is.EqualTo(new byte[] { 100, 101, 102 }));
				Assert.That(TestType<byte[]>					(conn, "blobDataType",      DataType.Blob),                Is.EqualTo(new byte[] { 100, 101, 102 }));

				Assert.That(TestType<ulong?>					(conn, "bitDataType"),                                     Is.EqualTo(5));
				Assert.That(TestType<string>					(conn, "enumDataType"),                                    Is.EqualTo("Green"));
				Assert.That(TestType<string>					(conn, "setDataType"),                                     Is.EqualTo("one"));

				if (context != ProviderName.MySqlConnector)
				{
					TestType<MySqlDataDecimal?>(conn, "decimalDataType", DataType.Decimal);

					var dt1 = TestType<MySqlDataDateTime?>(conn, "datetimeDataType", DataType.DateTime);
					var dt2 = new MySqlDataDateTime(2012, 12, 12, 12, 12, 12, 0)
					{
						TimezoneOffset = dt1.Value.TimezoneOffset
					};

					Assert.That(dt1, Is.EqualTo(dt2));
				}
				else
				{
					Assert.That(TestType<MySqlConnectorDateTime?>(conn, "datetimeDataType", DataType.DateTime), Is.EqualTo(new MySqlConnectorDateTime(2012, 12, 12, 12, 12, 12, 0)));
				}
			}
		}

		[Test]
		public void TestDate([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				var dateTime = new DateTime(2012, 12, 12);

				Assert.That(conn.Execute<DateTime> ("SELECT Cast('2012-12-12' as date)"),                          Is.EqualTo(dateTime));
				Assert.That(conn.Execute<DateTime?>("SELECT Cast('2012-12-12' as date)"),                          Is.EqualTo(dateTime));
				Assert.That(conn.Execute<DateTime> ("SELECT @p", DataParameter.Date("p", dateTime)),               Is.EqualTo(dateTime));
				Assert.That(conn.Execute<DateTime?>("SELECT @p", new DataParameter("p", dateTime, DataType.Date)), Is.EqualTo(dateTime));
			}
		}

		[Test]
		public void TestDateTime([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				var dateTime = new DateTime(2012, 12, 12, 12, 12, 12);

				Assert.That(conn.Execute<DateTime> ("SELECT Cast('2012-12-12 12:12:12' as datetime)"),                 Is.EqualTo(dateTime));
				Assert.That(conn.Execute<DateTime?>("SELECT Cast('2012-12-12 12:12:12' as datetime)"),                 Is.EqualTo(dateTime));

				Assert.That(conn.Execute<DateTime> ("SELECT @p", DataParameter.DateTime("p", dateTime)),               Is.EqualTo(dateTime));
				Assert.That(conn.Execute<DateTime?>("SELECT @p", new DataParameter("p", dateTime)),                    Is.EqualTo(dateTime));
				Assert.That(conn.Execute<DateTime?>("SELECT @p", new DataParameter("p", dateTime, DataType.DateTime)), Is.EqualTo(dateTime));
			}
		}

		[Test]
		public void TestChar([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<char> ("SELECT Cast('1' as char)"),         Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as char)"),         Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT Cast('1' as char(1))"),      Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast('1' as char(1))"),      Is.EqualTo('1'));

				Assert.That(conn.Execute<char> ("SELECT @p",                  DataParameter.Char("p",  '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p",                  DataParameter.Char("p",  '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT Cast(@p as char)",    DataParameter.Char("p",  '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast(@p as char)",    DataParameter.Char("p",  '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT Cast(@p as char(1))", DataParameter.Char("@p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT Cast(@p as char(1))", DataParameter.Char("@p", '1')), Is.EqualTo('1'));

				Assert.That(conn.Execute<char> ("SELECT @p", DataParameter.VarChar ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p", DataParameter.VarChar ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT @p", DataParameter.NChar   ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p", DataParameter.NChar   ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT @p", DataParameter.NVarChar("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p", DataParameter.NVarChar("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char> ("SELECT @p", DataParameter.Create  ("p", '1')), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p", DataParameter.Create  ("p", '1')), Is.EqualTo('1'));

				Assert.That(conn.Execute<char> ("SELECT @p", new DataParameter { Name = "p", Value = '1' }), Is.EqualTo('1'));
				Assert.That(conn.Execute<char?>("SELECT @p", new DataParameter { Name = "p", Value = '1' }), Is.EqualTo('1'));
			}
		}

		[Test]
		public void TestString([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<string>("SELECT Cast('12345' as char(20))"),      Is.EqualTo("12345"));
				Assert.That(conn.Execute<string>("SELECT Cast(NULL    as char(20))"),      Is.Null);

				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.Char    ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.VarChar ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.Text    ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.NChar   ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.NVarChar("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.NText   ("p", "123")), Is.EqualTo("123"));
				Assert.That(conn.Execute<string>("SELECT @p", DataParameter.Create  ("p", "123")), Is.EqualTo("123"));

				Assert.That(conn.Execute<string>("SELECT @p", new DataParameter { Name = "p", Value = "1" }), Is.EqualTo("1"));
			}
		}

		[Test]
		public void TestBinary([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			var arr1 = new byte[] { 48, 57 };

			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.Binary   ("p", arr1)),             Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.VarBinary("p", arr1)),             Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.Create   ("p", arr1)),             Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.VarBinary("p", null)),             Is.EqualTo(null));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.VarBinary("p", new byte[0])),      Is.EqualTo(new byte[0]));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.Image    ("p", new byte[0])),      Is.EqualTo(new byte[0]));
				Assert.That(conn.Execute<byte[]>("SELECT @p", new DataParameter { Name = "p", Value = arr1 }), Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", DataParameter.Create   ("p", new Binary(arr1))), Is.EqualTo(arr1));
				Assert.That(conn.Execute<byte[]>("SELECT @p", new DataParameter("p", new Binary(arr1))),       Is.EqualTo(arr1));
			}
		}

		[Test]
		public void TestXml([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<string>     ("SELECT '<xml/>'"),            Is.EqualTo("<xml/>"));
				Assert.That(conn.Execute<XDocument>  ("SELECT '<xml/>'").ToString(), Is.EqualTo("<xml />"));
				Assert.That(conn.Execute<XmlDocument>("SELECT '<xml/>'").InnerXml,   Is.EqualTo("<xml />"));

				var xdoc = XDocument.Parse("<xml/>");
				var xml  = Convert<string,XmlDocument>.Lambda("<xml/>");

				Assert.That(conn.Execute<string>     ("SELECT @p", DataParameter.Xml("p", "<xml/>")),        Is.EqualTo("<xml/>"));
				Assert.That(conn.Execute<XDocument>  ("SELECT @p", DataParameter.Xml("p", xdoc)).ToString(), Is.EqualTo("<xml />"));
				Assert.That(conn.Execute<XmlDocument>("SELECT @p", DataParameter.Xml("p", xml)). InnerXml,   Is.EqualTo("<xml />"));
				Assert.That(conn.Execute<XDocument>  ("SELECT @p", new DataParameter("p", xdoc)).ToString(), Is.EqualTo("<xml />"));
				Assert.That(conn.Execute<XDocument>  ("SELECT @p", new DataParameter("p", xml)). ToString(), Is.EqualTo("<xml />"));
			}
		}

		enum TestEnum
		{
			[MapValue("A")] AA,
			[MapValue("B")] BB,
		}

		[Test]
		public void TestEnum1([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<TestEnum> ("SELECT 'A'"), Is.EqualTo(TestEnum.AA));
				Assert.That(conn.Execute<TestEnum?>("SELECT 'A'"), Is.EqualTo(TestEnum.AA));
				Assert.That(conn.Execute<TestEnum> ("SELECT 'B'"), Is.EqualTo(TestEnum.BB));
				Assert.That(conn.Execute<TestEnum?>("SELECT 'B'"), Is.EqualTo(TestEnum.BB));
			}
		}

		[Test]
		public void TestEnum2([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var conn = new DataConnection(context))
			{
				Assert.That(conn.Execute<string>("SELECT @p", new { p = TestEnum.AA }),            Is.EqualTo("A"));
				Assert.That(conn.Execute<string>("SELECT @p", new { p = (TestEnum?)TestEnum.BB }), Is.EqualTo("B"));

				Assert.That(conn.Execute<string>("SELECT @p", new { p = ConvertTo<string>.From((TestEnum?)TestEnum.AA) }), Is.EqualTo("A"));
				Assert.That(conn.Execute<string>("SELECT @p", new { p = ConvertTo<string>.From(TestEnum.AA) }), Is.EqualTo("A"));
				Assert.That(conn.Execute<string>("SELECT @p", new { p = conn.MappingSchema.GetConverter<TestEnum?,string>()(TestEnum.AA) }), Is.EqualTo("A"));
			}
		}

		[Table("alltypes")]
		public partial class AllType
		{
			[PrimaryKey, Identity] public int       ID                  { get; set; } // int(11)
			[Column,     Nullable] public long?     bigintDataType      { get; set; } // bigint(20)
			[Column,     Nullable] public short?    smallintDataType    { get; set; } // smallint(6)
			[Column,     Nullable] public sbyte?    tinyintDataType     { get; set; } // tinyint(4)
			[Column,     Nullable] public int?      mediumintDataType   { get; set; } // mediumint(9)
			[Column,     Nullable] public int?      intDataType         { get; set; } // int(11)
			[Column,     Nullable] public decimal?  numericDataType     { get; set; } // decimal(10,0)
			[Column,     Nullable] public decimal?  decimalDataType     { get; set; } // decimal(10,0)
			[Column,     Nullable] public double?   doubleDataType      { get; set; } // double
			[Column,     Nullable] public float?    floatDataType       { get; set; } // float
			[Column,     Nullable] public DateTime? dateDataType        { get; set; } // date
			[Column,     Nullable] public DateTime? datetimeDataType    { get; set; } // datetime
			[Column,     Nullable] public DateTime? timestampDataType   { get; set; } // timestamp
			[Column,     Nullable] public TimeSpan? timeDataType        { get; set; } // time
			[Column,     Nullable] public int?      yearDataType        { get; set; } // year(4)
			[Column,     Nullable] public int?      year2DataType       { get; set; } // year(2)
			[Column,     Nullable] public int?      year4DataType       { get; set; } // year(4)
			[Column,     Nullable] public char?     charDataType        { get; set; } // char(1)
			[Column,     Nullable] public string    varcharDataType     { get; set; } // varchar(20)
			[Column,     Nullable] public string    textDataType        { get; set; } // text
			[Column,     Nullable] public byte[]    binaryDataType      { get; set; } // binary(3)
			[Column,     Nullable] public byte[]    varbinaryDataType   { get; set; } // varbinary(5)
			[Column,     Nullable] public byte[]    blobDataType        { get; set; } // blob
			[Column,     Nullable] public UInt64?   bitDataType         { get; set; } // bit(3)
			[Column,     Nullable] public string    enumDataType        { get; set; } // enum('Green','Red','Blue')
			[Column,     Nullable] public string    setDataType         { get; set; } // set('one','two')
			[Column,     Nullable] public uint?     intUnsignedDataType { get; set; } // int(10) unsigned
		}

		void BulkCopyTest(string context, BulkCopyType bulkCopyType)
		{
			using (var conn = new DataConnection(context))
			{
				conn.BeginTransaction();

				conn.BulkCopy(new BulkCopyOptions { MaxBatchSize = 50000, BulkCopyType = bulkCopyType },
					Enumerable.Range(0, 100000).Select(n =>
						new AllType
						{
							ID                  = 2000 + n,
							bigintDataType      = 3000 + n,
							smallintDataType    = (short)(4000 + n),
							tinyintDataType     = (sbyte)(5000 + n),
							mediumintDataType   = 6000 + n,
							intDataType         = 7000 + n,
							numericDataType     = 8000 + n,
							decimalDataType     = 9000 + n,
							doubleDataType      = 8800 + n,
							floatDataType       = 7700 + n,
							dateDataType        = DateTime.Now,
							datetimeDataType    = DateTime.Now,
							timestampDataType   = null,
							timeDataType        = null,
							yearDataType        = (1000 + n) % 100,
							year2DataType       = (1000 + n) % 100,
							year4DataType       = null,
							charDataType        = 'A',
							varcharDataType     = "",
							textDataType        = "",
							binaryDataType      = null,
							varbinaryDataType   = null,
							blobDataType        = new byte[] { 1, 2, 3 },
							bitDataType         = null,
							enumDataType        = "Green",
							setDataType         = "one",
							intUnsignedDataType = (uint)(5000 + n),
						}));

				//var list = conn.GetTable<ALLTYPE>().ToList();

				conn.GetTable<ALLTYPE>().Delete(p => p.SMALLINTDATATYPE >= 5000);
			}
		}

		[Test, Explicit("It works too long.")]
		public void BulkCopyMultipleRows([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			BulkCopyTest(context, BulkCopyType.MultipleRows);
		}

		[Test, Explicit("It works too long.")]
		public void BulkCopyRetrieveSequencesMultipleRows([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			BulkCopyRetrieveSequence(context, BulkCopyType.MultipleRows);
		}

		[Test, Explicit("It works too long.")]
		public void BulkCopyProviderSpecific([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			BulkCopyTest(context, BulkCopyType.ProviderSpecific);
		}

		[Test, Explicit("It works too long.")]
		public void BulkCopyRetrieveSequencesProviderSpecific([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			BulkCopyRetrieveSequence(context, BulkCopyType.ProviderSpecific);
		}

		[Test]
		public void BulkCopyLinqTypes([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			foreach (var bulkCopyType in new[] { BulkCopyType.MultipleRows, BulkCopyType.ProviderSpecific })
			{
				using (var db = new DataConnection(context))
				{
					db.BulkCopy(
						new BulkCopyOptions { BulkCopyType = bulkCopyType },
						Enumerable.Range(0, 10).Select(n =>
						new LinqDataTypes
						{
							ID = 4000 + n,
							MoneyValue = 1000m + n,
							DateTimeValue = new DateTime(2001, 1, 11, 1, 11, 21, 100),
							BoolValue = true,
							GuidValue = Guid.NewGuid(),
							SmallIntValue = (short)n
						}));
					db.GetTable<LinqDataTypes>().Delete(p => p.ID >= 4000);
				}
			}
		}

		static void BulkCopyRetrieveSequence(string context, BulkCopyType bulkCopyType)
		{
			var data = new[]
			{
				new Doctor { Taxonomy = "Neurologist"},
				new Doctor { Taxonomy = "Sports Medicine"},
				new Doctor { Taxonomy = "Optometrist"},
				new Doctor { Taxonomy = "Pediatrics" },
				new Doctor { Taxonomy = "Psychiatry" }
			};

			using (var db = new TestDataConnection(context))
			{
				var options = new BulkCopyOptions
				{
					MaxBatchSize = 5,
					//RetrieveSequence = true,
					KeepIdentity = true,
					BulkCopyType = bulkCopyType,
					NotifyAfter  = 3,
					RowsCopiedCallback = copied => Debug.WriteLine(copied.RowsCopied)
				};
				db.BulkCopy(options, data.RetrieveIdentity(db));

				foreach (var d in data)
					Assert.That(d.PersonID, Is.GreaterThan(0));
			}
		}

		[Test]
		public void TestTransaction1([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var db = new DataConnection(context))
			{
				db.GetTable<Parent>().Update(p => p.ParentID == 1, p => new Parent { Value1 = 1 });

				db.BeginTransaction();

				db.GetTable<Parent>().Update(p => p.ParentID == 1, p => new Parent { Value1 = null });

				Assert.IsNull(db.GetTable<Parent>().First(p => p.ParentID == 1).Value1);

				db.RollbackTransaction();

				Assert.That(1, Is.EqualTo(db.GetTable<Parent>().First(p => p.ParentID == 1).Value1));
			}
		}

		[Test]
		public void TestTransaction2([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var db = new DataConnection(context))
			{
				db.GetTable<Parent>().Update(p => p.ParentID == 1, p => new Parent { Value1 = 1 });

				using (var tran = db.BeginTransaction())
				{
					db.GetTable<Parent>().Update(p => p.ParentID == 1, p => new Parent { Value1 = null });

					Assert.IsNull(db.GetTable<Parent>().First(p => p.ParentID == 1).Value1);

					tran.Rollback();

					Assert.That(1, Is.EqualTo(db.GetTable<Parent>().First(p => p.ParentID == 1).Value1));
				}
			}
		}

#if !NETSTANDARD1_6 && !NETSTANDARD2_0
		[Test]
		public void SchemaProviderTest([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var db = (DataConnection)GetDataContext(context))
			{
				var sp = db.DataProvider.GetSchemaProvider();
				var schema = sp.GetSchema(db, TestUtils.GetDefaultSchemaOptions(context));

				var systemTables = schema.Tables.Where(_ => _.CatalogName.Equals("sys", StringComparison.OrdinalIgnoreCase)).ToList();

				Assert.That(systemTables.All(_ => _.IsProviderSpecific));

				var views = schema.Tables.Where(_ => _.IsView).ToList();
				Assert.AreEqual(1, views.Count);
			}
		}

		public static IEnumerable<ProcedureSchema> ProcedureTestCases
		{
			get
			{
				// create procedure
				yield return new ProcedureSchema()
				{
					CatalogName     = "SET_BY_TEST",
					ProcedureName   = "TestProcedure",
					MemberName      = "TestProcedure",
					IsDefaultSchema = true,
					IsLoaded        = true,
					Parameters      = new List<ParameterSchema>()
					{
						new ParameterSchema()
						{
							SchemaName    = "param3",
							SchemaType    = "INT",
							IsIn          = true,
							ParameterName = "param3",
							ParameterType = "int?",
							SystemType    = typeof(int),
							DataType      = DataType.Int32
						},
						new ParameterSchema()
						{
							SchemaName    = "param2",
							SchemaType    = "INT",
							IsIn          = true,
							IsOut         = true,
							ParameterName = "param2",
							ParameterType = "int?",
							SystemType    = typeof(int),
							DataType      = DataType.Int32
						},
						new ParameterSchema()
						{
							SchemaName    = "param1",
							SchemaType    = "INT",
							IsOut         = true,
							ParameterName = "param1",
							ParameterType = "int?",
							SystemType    = typeof(int),
							DataType      = DataType.Int32
						}
					},
					ResultTable = new TableSchema()
					{
						IsProcedureResult = true,
						TypeName          = "TestProcedureResult",
						Columns           = new List<ColumnSchema>()
						{
							new ColumnSchema()
							{
								ColumnName = "PersonID",
								ColumnType = "INT",
								MemberName = "PersonID",
								MemberType = "int",
								SystemType = typeof(int),
								DataType   = DataType.Int32
							},
							new ColumnSchema()
							{
								ColumnName = "FirstName",
								ColumnType = "VARCHAR(50)",
								MemberName = "FirstName",
								MemberType = "string",
								SystemType = typeof(string),
								DataType   = DataType.VarChar
							},
							new ColumnSchema()
							{
								ColumnName = "LastName",
								ColumnType = "VARCHAR(50)",
								MemberName = "LastName",
								MemberType = "string",
								SystemType = typeof(string),
								DataType   = DataType.VarChar
							},
							new ColumnSchema()
							{
								ColumnName = "MiddleName",
								ColumnType = "VARCHAR(50)",
								IsNullable = true,
								MemberName = "MiddleName",
								MemberType = "string",
								SystemType = typeof(string),
								DataType   = DataType.VarChar
							},
							new ColumnSchema()
							{
								ColumnName = "Gender",
								ColumnType = "CHAR(1)",
								MemberName = "Gender",
								MemberType = "char",
								SystemType = typeof(char),
								DataType   = DataType.Char
							}
						}
					},
					SimilarTables = new List<TableSchema>()
					{
						new TableSchema()
						{
							TableName = "person"
						}
					}
				};

				// create function
				yield return new ProcedureSchema()
				{
					CatalogName     = "SET_BY_TEST",
					ProcedureName   = "TestFunction",
					MemberName      = "TestFunction",
					IsFunction      = true,
					IsDefaultSchema = true,
					Parameters      = new List<ParameterSchema>()
					{
						new ParameterSchema()
						{
							SchemaType    = "VARCHAR",
							IsResult      = true,
							ParameterName = "par1",
							ParameterType = "string",
							SystemType    = typeof(string),
							DataType      = DataType.VarChar
						},
						new ParameterSchema()
						{
							SchemaName    = "param",
							SchemaType    = "INT",
							IsIn          = true,
							ParameterName = "param",
							ParameterType = "int?",
							SystemType    = typeof(int),
							DataType      = DataType.Int32
						}
					}
				};

				// create function
				yield return new ProcedureSchema()
				{
					CatalogName     = "SET_BY_TEST",
					ProcedureName   = "TestOutputParametersWithoutTableProcedure",
					MemberName      = "TestOutputParametersWithoutTableProcedure",
					IsDefaultSchema = true,
					IsLoaded        = true,
					Parameters      = new List<ParameterSchema>()
					{
						new ParameterSchema()
						{
							SchemaName    = "aInParam",
							SchemaType    = "VARCHAR",
							IsIn          = true,
							ParameterName = "aInParam",
							ParameterType = "string",
							SystemType    = typeof(string),
							DataType      = DataType.VarChar
						},
						new ParameterSchema()
						{
							SchemaName    = "aOutParam",
							SchemaType    = "TINYINT",
							IsOut         = true,
							ParameterName = "aOutParam",
							ParameterType = "sbyte?",
							SystemType    = typeof(sbyte),
							DataType      = DataType.SByte
						}
					}
				};
			}
		}

		[Test]
		public void ProceduresSchemaProviderTest(
			[IncludeDataSources(TestProvName.AllMySql)] string context,
			[ValueSource(nameof(ProcedureTestCases))] ProcedureSchema expectedProc)
		{
			// TODO: add aggregate/udf functions test cases
			using (var db = (DataConnection)GetDataContext(context))
			{
				expectedProc.CatalogName = TestUtils.GetDatabaseName(db);

				var schema = db.DataProvider.GetSchemaProvider().GetSchema(db, TestUtils.GetDefaultSchemaOptions(context));

				var procedures = schema.Procedures.Where(_ => _.ProcedureName == expectedProc.ProcedureName).ToList();

				Assert.AreEqual(1, procedures.Count);

				var procedure = procedures[0];

				Assert.AreEqual(expectedProc.CatalogName.ToLower(), procedure.CatalogName.ToLower());
				Assert.AreEqual(expectedProc.SchemaName,            procedure.SchemaName);
				Assert.AreEqual(expectedProc.MemberName,            procedure.MemberName);
				Assert.AreEqual(expectedProc.IsTableFunction,       procedure.IsTableFunction);
				Assert.AreEqual(expectedProc.IsAggregateFunction,   procedure.IsAggregateFunction);
				Assert.AreEqual(expectedProc.IsDefaultSchema,       procedure.IsDefaultSchema);
				Assert.AreEqual(expectedProc.IsLoaded,              procedure.IsLoaded);

				Assert.IsNull(procedure.ResultException);

				Assert.AreEqual(expectedProc.Parameters.Count, procedure.Parameters.Count);

				for (var i = 0; i < procedure.Parameters.Count; i++)
				{
					var actualParam = procedure.Parameters[i];
					var expectedParam = expectedProc.Parameters[i];

					Assert.IsNotNull(expectedParam);

					Assert.AreEqual(expectedParam.SchemaName,           actualParam.SchemaName);
					Assert.AreEqual(expectedParam.ParameterName,        actualParam.ParameterName);
					Assert.AreEqual(expectedParam.SchemaType,           actualParam.SchemaType);
					Assert.AreEqual(expectedParam.IsIn,                 actualParam.IsIn);
					Assert.AreEqual(expectedParam.IsOut,                actualParam.IsOut);
					Assert.AreEqual(expectedParam.IsResult,             actualParam.IsResult);
					Assert.AreEqual(expectedParam.Size,                 actualParam.Size);
					Assert.AreEqual(expectedParam.ParameterType,        actualParam.ParameterType);
					Assert.AreEqual(expectedParam.SystemType,           actualParam.SystemType);
					Assert.AreEqual(expectedParam.DataType,             actualParam.DataType);
					Assert.AreEqual(expectedParam.ProviderSpecificType, actualParam.ProviderSpecificType);
				}

				if (expectedProc.ResultTable == null)
				{
					Assert.IsNull(procedure.ResultTable);

					// maybe it is worth changing
					Assert.IsNull(procedure.SimilarTables);
				}
				else
				{
					Assert.IsNotNull(procedure.ResultTable);

					var expectedTable = expectedProc.ResultTable;
					var actualTable = procedure.ResultTable;

					Assert.AreEqual(expectedTable.ID,                 actualTable.ID);
					Assert.AreEqual(expectedTable.CatalogName,        actualTable.CatalogName);
					Assert.AreEqual(expectedTable.SchemaName,         actualTable.SchemaName);
					Assert.AreEqual(expectedTable.TableName,          actualTable.TableName);
					Assert.AreEqual(expectedTable.Description,        actualTable.Description);
					Assert.AreEqual(expectedTable.IsDefaultSchema,    actualTable.IsDefaultSchema);
					Assert.AreEqual(expectedTable.IsView,             actualTable.IsView);
					Assert.AreEqual(expectedTable.IsProcedureResult,  actualTable.IsProcedureResult);
					Assert.AreEqual(expectedTable.TypeName,           actualTable.TypeName);
					Assert.AreEqual(expectedTable.IsProviderSpecific, actualTable.IsProviderSpecific);

					Assert.IsNotNull(actualTable.ForeignKeys);
					Assert.IsEmpty(actualTable.ForeignKeys);

					Assert.AreEqual(expectedTable.Columns.Count, actualTable.Columns.Count);

					foreach (var actualColumn in actualTable.Columns)
					{
						var expectedColumn = expectedTable.Columns
							.Where(_ => _.ColumnName == actualColumn.ColumnName)
							.SingleOrDefault();

						Assert.IsNotNull(expectedColumn);

						Assert.AreEqual(expectedColumn.ColumnType,           actualColumn.ColumnType);
						Assert.AreEqual(expectedColumn.IsNullable,           actualColumn.IsNullable);
						Assert.AreEqual(expectedColumn.IsIdentity,           actualColumn.IsIdentity);
						Assert.AreEqual(expectedColumn.IsPrimaryKey,         actualColumn.IsPrimaryKey);
						Assert.AreEqual(expectedColumn.PrimaryKeyOrder,      actualColumn.PrimaryKeyOrder);
						Assert.AreEqual(expectedColumn.Description,          actualColumn.Description);
						Assert.AreEqual(expectedColumn.MemberName,           actualColumn.MemberName);
						Assert.AreEqual(expectedColumn.MemberType,           actualColumn.MemberType);
						Assert.AreEqual(expectedColumn.ProviderSpecificType, actualColumn.ProviderSpecificType);
						Assert.AreEqual(expectedColumn.SystemType,           actualColumn.SystemType);
						Assert.AreEqual(expectedColumn.DataType,             actualColumn.DataType);
						Assert.AreEqual(expectedColumn.SkipOnInsert,         actualColumn.SkipOnInsert);
						Assert.AreEqual(expectedColumn.SkipOnUpdate,         actualColumn.SkipOnUpdate);
						Assert.AreEqual(expectedColumn.Length,               actualColumn.Length);
						Assert.AreEqual(expectedColumn.Precision,            actualColumn.Precision);
						Assert.AreEqual(expectedColumn.Scale,                actualColumn.Scale);
						Assert.AreEqual(actualTable,                         actualColumn.Table);
					}

					Assert.IsNotNull(procedure.SimilarTables);

					foreach (var table in procedure.SimilarTables)
					{
						var tbl = expectedProc.SimilarTables
							.SingleOrDefault(_ => _.TableName.ToLower() == table.TableName.ToLower());

						Assert.IsNotNull(tbl);
					}
				}
			}
		}

		[Test]
		public void FullTextIndexTest([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var db = (DataConnection)GetDataContext(context))
			{
				DatabaseSchema schema = db.DataProvider.GetSchemaProvider().GetSchema(db, TestUtils.GetDefaultSchemaOptions(context));
				var res = schema.Tables.FirstOrDefault(c => c.ID.ToLower().Contains("fulltextindex"));
				Assert.AreNotEqual(null, res);
			}
		}

#endif

		[Sql.Expression("@n:=@n+1", ServerSideOnly = true)]
		static int IncrementIndex()
		{
			throw new NotImplementedException();
		}

		[Description("https://stackoverflow.com/questions/50858172/linq2db-mysql-set-row-index/50958483")]
		[Test]
		public void RowIndexTest([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var db = GetDataContext(context))
			{
				db.NextQueryHints.Add("**/*(SELECT @n := 0) `rowcounter`*/");
				db.NextQueryHints.Add(", (SELECT @n := 0) `rowcounter`");

				var q =
					from p in db.Person
					select new
					{
						rank = IncrementIndex(),
						id   = p.ID
					};

				var list = q.ToList();
			}
		}

		[Test]
		public void TestTestProcedure([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var db = (DataConnection)GetDataContext(context))
			{
				int? param2 = 5;
				int? param1 = 11;

				var res = db.TestProcedure(123, ref param2, out param1);

				Assert.AreEqual(10, param2);
				Assert.AreEqual(133, param1);
				AreEqual(db.GetTable<Person>(), res);
			}
		}

		[Test]
		public void TestTestOutputParametersWithoutTableProcedure([IncludeDataSources(TestProvName.AllMySql)] string context)
		{
			using (var db = (DataConnection)GetDataContext(context))
			{
				var res = db.TestOutputParametersWithoutTableProcedure("test", out var outParam);

				Assert.AreEqual(123, outParam);
				Assert.AreEqual(1, res);
			}
		}

		[Table]
		public class CreateTable
		{
			[Column                                                 ] public string VarChar255;
			[Column(Length = 1)                                     ] public string VarChar1;
			[Column(Length = 112)                                   ] public string VarChar112;
			[Column                                                 ] public char Char;
			[Column(DataType = DataType.Char)                       ] public string Char255;
			[Column(DataType = DataType.Char, Length = 1)           ] public string Char1;
			[Column(DataType = DataType.Char, Length = 112)         ] public string Char112;
			[Column(Length = 1)                                     ] public byte[] VarBinary1;
			[Column                                                 ] public byte[] VarBinary255;
			[Column(Length = 3)                                     ] public byte[] VarBinary3;
			[Column(DataType = DataType.Binary, Length = 1)         ] public byte[] Binary1;
			[Column(DataType = DataType.Binary)                     ] public byte[] Binary255;
			[Column(DataType = DataType.Binary, Length = 3)         ] public byte[] Binary3;
			[Column(DataType = DataType.Blob, Length = 200)         ] public byte[] TinyBlob;
			[Column(DataType = DataType.Blob, Length = 2000)        ] public byte[] Blob;
			[Column(DataType = DataType.Blob, Length = 200000)      ] public byte[] MediumBlob;
			[Column(DataType = DataType.Blob)                       ] public byte[] BlobDefault;
			[Column(DataType = DataType.Blob, Length = int.MaxValue)] public byte[] LongBlob;
			[Column(DataType = DataType.Text, Length = 200)         ] public string TinyText;
			[Column(DataType = DataType.Text, Length = 2000)        ] public string Text;
			[Column(DataType = DataType.Text, Length = 200000)      ] public string MediumText;
			[Column(DataType = DataType.Text, Length = int.MaxValue)] public string LongText;
			[Column(DataType = DataType.Text)                       ] public string TextDefault;
			[Column(DataType = DataType.Date)                       ] public DateTime Date;
			[Column                                                 ] public DateTime DateTime;
			[Column(Precision = 3)                                  ] public DateTime DateTime3;
			// MySQL.Data provider has issues with timestamps
			// TODO: look into it later
			[Column(Configuration = ProviderName.MySqlConnector)    ] public DateTimeOffset TimeStamp;
			[Column(Precision = 5, Configuration = ProviderName.MySqlConnector)] public DateTimeOffset TimeStamp5;
			[Column                                                 ] public TimeSpan Time;
			[Column(Precision = 2)                                  ] public TimeSpan Time2;
			[Column                                                 ] public sbyte TinyInt;
			[Column                                                 ] public byte UnsignedTinyInt;
			[Column                                                 ] public short SmallInt;
			[Column                                                 ] public ushort UnsignedSmallInt;
			[Column                                                 ] public int Int;
			[Column                                                 ] public uint UnsignedInt;
			[Column                                                 ] public long BigInt;
			[Column                                                 ] public ulong UnsignedBigInt;
			[Column                                                 ] public decimal Decimal;
			[Column(Precision = 15)                                 ] public decimal Decimal15_0;
			[Column(Scale = 5)                                      ] public decimal Decimal10_5;
			[Column(Precision = 20, Scale = 2)                      ] public decimal Decimal20_2;
			[Column                                                 ] public float Float;
			[Column(Precision = 10)                                 ] public float Float10;
			[Column                                                 ] public double Double;
			[Column(Precision = 30)                                 ] public double Float30;
			[Column                                                 ] public bool Bool;
			[Column(DataType = DataType.BitArray)                   ] public bool Bit1;
			[Column(DataType = DataType.BitArray)                   ] public byte Bit8;
			[Column(DataType = DataType.BitArray)                   ] public short Bit16;
			[Column(DataType = DataType.BitArray)                   ] public int Bit32;
			[Column(DataType = DataType.BitArray, Length = 10)      ] public int Bit10;
			[Column(DataType = DataType.BitArray)                   ] public long Bit64;
			[Column(DataType = DataType.Json)                       ] public string Json;
			// not mysql type, just mapping testing
			[Column                                                 ] public Guid Guid;
		}

		// for Travis use old mysql version
		[Category("SkipCI")]
		[ActiveIssue(Configuration = ProviderName.MySql, Details = "Disable test for MySql test provider, as it use old mysql version, which is fixed in 3.0 branch")]
		[Test]
		public void TestCreateTable([IncludeDataSources(false, TestProvName.AllMySql)] string context)
		{
			var isMySqlConnector = context == ProviderName.MySqlConnector;

			// TODO: Following types not mapped to DataType enum now and should be defined explicitly using DbType:
			// - ENUM      : https://dev.mysql.com/doc/refman/8.0/en/enum.html
			// - SET       : https://dev.mysql.com/doc/refman/8.0/en/set.html
			// - YEAR      : https://dev.mysql.com/doc/refman/8.0/en/year.html
			// - MEDIUMINT : https://dev.mysql.com/doc/refman/8.0/en/integer-types.html
			// - SERIAL    : https://dev.mysql.com/doc/refman/8.0/en/numeric-type-syntax.html
			// - spatial types : https://dev.mysql.com/doc/refman/8.0/en/spatial-type-overview.html
			// - any additional attributes for column create clause
			//
			// Also we deliberatly don't support various deprecated modifiers:
			// - display width
			// - unsigned for non-integer types
			// - floating point (M,D) specifiers
			// - synonyms (except BOOLEAN)
			// etc
			using (var db = new TestDataConnection(context))
			using (var table = db.CreateLocalTable<CreateTable>())
			{
				var sql = db.LastQuery;

				Assert.True(sql.Contains("\t`VarChar255`       VARCHAR(255)          NULL"));
				Assert.True(sql.Contains("\t`VarChar1`         VARCHAR(1)            NULL"));
				Assert.True(sql.Contains("\t`VarChar112`       VARCHAR(112)          NULL"));
				Assert.True(sql.Contains("\t`Char`             CHAR              NOT NULL"));
				Assert.True(sql.Contains("\t`Char1`            CHAR                  NULL"));
				Assert.True(sql.Contains("\t`Char255`          CHAR(255)             NULL"));
				Assert.True(sql.Contains("\t`Char112`          CHAR(112)             NULL"));
				Assert.True(sql.Contains("\t`VarBinary1`       VARBINARY(1)          NULL"));
				Assert.True(sql.Contains("\t`VarBinary255`     VARBINARY(255)        NULL"));
				Assert.True(sql.Contains("\t`VarBinary3`       VARBINARY(3)          NULL"));
				Assert.True(sql.Contains("\t`Binary1`          BINARY                NULL"));
				Assert.True(sql.Contains("\t`Binary255`        BINARY(255)           NULL"));
				Assert.True(sql.Contains("\t`Binary3`          BINARY(3)             NULL"));
				Assert.True(sql.Contains("\t`TinyBlob`         TINYBLOB              NULL"));
				Assert.True(sql.Contains("\t`Blob`             BLOB                  NULL"));
				Assert.True(sql.Contains("\t`MediumBlob`       MEDIUMBLOB            NULL"));
				Assert.True(sql.Contains("\t`LongBlob`         LONGBLOB              NULL"));
				Assert.True(sql.Contains("\t`BlobDefault`      BLOB                  NULL"));
				Assert.True(sql.Contains("\t`TinyText`         TINYTEXT              NULL"));
				Assert.True(sql.Contains("\t`Text`             TEXT                  NULL"));
				Assert.True(sql.Contains("\t`MediumText`       MEDIUMTEXT            NULL"));
				Assert.True(sql.Contains("\t`LongText`         LONGTEXT              NULL"));
				Assert.True(sql.Contains("\t`TextDefault`      TEXT                  NULL"));
				Assert.True(sql.Contains("\t`Date`             DATE              NOT NULL"));
				Assert.True(sql.Contains("\t`DateTime`         DATETIME          NOT NULL"));
				Assert.True(sql.Contains("\t`DateTime3`        DATETIME(3)       NOT NULL"));
				if (isMySqlConnector)
				{
					Assert.True(sql.Contains("\t`TimeStamp`        TIMESTAMP         NOT NULL"));
					Assert.True(sql.Contains("\t`TimeStamp5`       TIMESTAMP(5)      NOT NULL"));
				}
				Assert.True(sql.Contains("\t`Time`             TIME              NOT NULL"));
				Assert.True(sql.Contains("\t`Time2`            TIME(2)           NOT NULL"));
				Assert.True(sql.Contains("\t`TinyInt`          TINYINT           NOT NULL"));
				Assert.True(sql.Contains("\t`UnsignedTinyInt`  TINYINT UNSIGNED  NOT NULL"));
				Assert.True(sql.Contains("\t`SmallInt`         SMALLINT          NOT NULL"));
				Assert.True(sql.Contains("\t`UnsignedSmallInt` SMALLINT UNSIGNED NOT NULL"));
				Assert.True(sql.Contains("\t`Int`              INT               NOT NULL"));
				Assert.True(sql.Contains("\t`UnsignedInt`      INT UNSIGNED      NOT NULL"));
				Assert.True(sql.Contains("\t`BigInt`           BIGINT            NOT NULL"));
				Assert.True(sql.Contains("\t`UnsignedBigInt`   BIGINT UNSIGNED   NOT NULL"));
				Assert.True(sql.Contains("\t`Decimal`          DECIMAL           NOT NULL"));
				Assert.True(sql.Contains("\t`Decimal15_0`      DECIMAL(15)       NOT NULL"));
				Assert.True(sql.Contains("\t`Decimal10_5`      DECIMAL(10,5)     NOT NULL"));
				Assert.True(sql.Contains("\t`Decimal20_2`      DECIMAL(20,2)     NOT NULL"));
				Assert.True(sql.Contains("\t`Float`            FLOAT             NOT NULL"));
				Assert.True(sql.Contains("\t`Float10`          FLOAT(10)         NOT NULL"));
				Assert.True(sql.Contains("\t`Double`           DOUBLE            NOT NULL"));
				Assert.True(sql.Contains("\t`Float30`          FLOAT(30)         NOT NULL"));
				Assert.True(sql.Contains("\t`Bool`             BOOLEAN           NOT NULL"));
				Assert.True(sql.Contains("\t`Bit1`             BIT               NOT NULL"));
				Assert.True(sql.Contains("\t`Bit8`             BIT(8)            NOT NULL"));
				Assert.True(sql.Contains("\t`Bit16`            BIT(16)           NOT NULL"));
				Assert.True(sql.Contains("\t`Bit32`            BIT(32)           NOT NULL"));
				Assert.True(sql.Contains("\t`Bit10`            BIT(10)           NOT NULL"));
				Assert.True(sql.Contains("\t`Bit64`            BIT(64)           NOT NULL"));
				Assert.True(sql.Contains("\t`Json`             JSON                  NULL"));
				Assert.True(sql.Contains("\t`Guid`             CHAR(36)          NOT NULL"));

				var testRecord = new CreateTable()
				{
					VarChar1         = "ы",
					VarChar255       = "ыsdf",
					VarChar112       = "ы123",
					Char             = 'я',
					Char1            = "!",
					Char255          = "!sdg3@",
					Char112          = "123 fd",
					VarBinary1       = new byte[] { 1 },
					VarBinary255     = new byte[] { 1, 4, 22 },
					VarBinary3       = new byte[] { 1, 2, 4 },
					Binary1          = new byte[] { 22 },
					Binary255        = new byte[] { 22, 44, 21 },
					Binary3          = new byte[] { 1, 33 },
					TinyBlob         = new byte[] { 3, 2, 1 },
					Blob             = new byte[] { 13, 2, 1 },
					MediumBlob       = new byte[] { 23, 2, 1 },
					BlobDefault      = new byte[] { 33, 2, 1 },
					LongBlob         = new byte[] { 133, 2, 1 },
					TinyText         = "12я3",
					Text             = "1232354",
					MediumText       = "1df3",
					LongText         = "1v23",
					TextDefault      = "12 #3",
					Date             = new DateTime(2123, 2, 3),
					DateTime         = new DateTime(2123, 2, 3, 11, 22, 33),
					DateTime3        = new DateTime(2123, 2, 3, 11, 22, 33, 123),
					TimeStamp        = new DateTimeOffset(2023, 2, 3, 11, 22, 33, TimeSpan.FromMinutes(60)),
					TimeStamp5       = new DateTimeOffset(2013, 2, 3, 11, 22, 33, 123, TimeSpan.FromMinutes(-60)).AddTicks(45000),
					Time             = new TimeSpan(-5, 56, 7),
					Time2            = new TimeSpan(5, 56, 7, 12),
					TinyInt          = -123,
					UnsignedTinyInt  = 223,
					SmallInt         = short.MinValue,
					UnsignedSmallInt = ushort.MaxValue,
					Int              = int.MinValue,
					UnsignedInt      = uint.MaxValue,
					BigInt           = long.MinValue,
					UnsignedBigInt   = ulong.MaxValue,
					Decimal          = 1234m,
					Decimal15_0      = 123456789012345m,
					Decimal10_5      = -12345.2345m,
					Decimal20_2      = -3412345.23m,
					Float            = 3244.23999f,
					Float10          = 124.354f,
					Double           = 452.23523d,
					Float30          = 332.235d,
					Bool             = true,
					Bit1             = true,
					Bit8             = 0x07,
					Bit16            = 0xFE,
					Bit32            = 0xADFE,
					Bit10            = 0x003F,
					Bit64            = 0xDEADBEAF,
					Json             = "{\"x\": 10}",
					Guid             = Guid.NewGuid()
				};

				db.Insert(testRecord);
				var readRecord = table.Single();

				Assert.AreEqual(testRecord.VarChar1        , readRecord.VarChar1);
				Assert.AreEqual(testRecord.VarChar255      , readRecord.VarChar255);
				Assert.AreEqual(testRecord.VarChar112      , readRecord.VarChar112);
				Assert.AreEqual(testRecord.Char            , readRecord.Char);
				Assert.AreEqual(testRecord.Char1           , readRecord.Char1);
				Assert.AreEqual(testRecord.Char255         , readRecord.Char255);
				Assert.AreEqual(testRecord.Char112         , readRecord.Char112);
				Assert.AreEqual(testRecord.VarBinary1      , readRecord.VarBinary1);
				Assert.AreEqual(testRecord.VarBinary255    , readRecord.VarBinary255);
				Assert.AreEqual(testRecord.VarBinary3      , readRecord.VarBinary3);
				Assert.AreEqual(testRecord.Binary1         , readRecord.Binary1);
				// we trim padding only from char fields
				Assert.AreEqual(testRecord.Binary255.Concat(new byte[252]), readRecord.Binary255);
				Assert.AreEqual(testRecord.Binary3.Concat(new byte[1]), readRecord.Binary3);
				Assert.AreEqual(testRecord.TinyBlob        , readRecord.TinyBlob);
				Assert.AreEqual(testRecord.Blob            , readRecord.Blob);
				Assert.AreEqual(testRecord.MediumBlob      , readRecord.MediumBlob);
				Assert.AreEqual(testRecord.BlobDefault     , readRecord.BlobDefault);
				Assert.AreEqual(testRecord.LongBlob        , readRecord.LongBlob);
				Assert.AreEqual(testRecord.TinyText        , readRecord.TinyText);
				Assert.AreEqual(testRecord.Text            , readRecord.Text);
				Assert.AreEqual(testRecord.MediumText      , readRecord.MediumText);
				Assert.AreEqual(testRecord.LongText        , readRecord.LongText);
				Assert.AreEqual(testRecord.TextDefault     , readRecord.TextDefault);
				Assert.AreEqual(testRecord.Date            , readRecord.Date);
				Assert.AreEqual(testRecord.DateTime        , readRecord.DateTime);
				Assert.AreEqual(testRecord.DateTime3       , readRecord.DateTime3);
				if (isMySqlConnector)
				{
					Assert.AreEqual(testRecord.TimeStamp,  readRecord.TimeStamp);
					Assert.AreEqual(testRecord.TimeStamp5, readRecord.TimeStamp5);
				}
				Assert.AreEqual(testRecord.Time            , readRecord.Time);
				Assert.AreEqual(testRecord.Time2           , readRecord.Time2);
				Assert.AreEqual(testRecord.TinyInt         , readRecord.TinyInt);
				Assert.AreEqual(testRecord.UnsignedTinyInt , readRecord.UnsignedTinyInt);
				Assert.AreEqual(testRecord.SmallInt        , readRecord.SmallInt);
				Assert.AreEqual(testRecord.UnsignedSmallInt, readRecord.UnsignedSmallInt);
				Assert.AreEqual(testRecord.Int             , readRecord.Int);
				Assert.AreEqual(testRecord.UnsignedInt     , readRecord.UnsignedInt);
				Assert.AreEqual(testRecord.BigInt          , readRecord.BigInt);
				Assert.AreEqual(testRecord.UnsignedBigInt  , readRecord.UnsignedBigInt);
				Assert.AreEqual(testRecord.Decimal         , readRecord.Decimal);
				Assert.AreEqual(testRecord.Decimal15_0     , readRecord.Decimal15_0);
				Assert.AreEqual(testRecord.Decimal10_5     , readRecord.Decimal10_5);
				Assert.AreEqual(testRecord.Decimal20_2     , readRecord.Decimal20_2);
				Assert.AreEqual(testRecord.Float           , readRecord.Float);
				Assert.AreEqual(testRecord.Float10         , readRecord.Float10);
				Assert.AreEqual(testRecord.Double          , readRecord.Double);
				Assert.AreEqual(testRecord.Float30         , readRecord.Float30);
				Assert.AreEqual(testRecord.Bool            , readRecord.Bool);
				Assert.AreEqual(testRecord.Bit1            , readRecord.Bit1);
				Assert.AreEqual(testRecord.Bit8            , readRecord.Bit8);
				Assert.AreEqual(testRecord.Bit16           , readRecord.Bit16);
				Assert.AreEqual(testRecord.Bit32           , readRecord.Bit32);
				Assert.AreEqual(testRecord.Bit10           , readRecord.Bit10);
				Assert.AreEqual(testRecord.Bit64           , readRecord.Bit64);
				Assert.AreEqual(testRecord.Json            , readRecord.Json);
				Assert.AreEqual(testRecord.Guid            , readRecord.Guid);
			}
		}
	}

	internal static class MySqlTestFunctions
	{
		public static int TestOutputParametersWithoutTableProcedure(this DataConnection dataConnection, string aInParam, out sbyte? aOutParam)
		{
			var ret = dataConnection.ExecuteProc("`TestOutputParametersWithoutTableProcedure`",
				new DataParameter("aInParam", aInParam, DataType.VarChar),
				new DataParameter("aOutParam", null, DataType.SByte) { Direction = ParameterDirection.Output });

			aOutParam = Converter.ChangeTypeTo<sbyte?>(((IDbDataParameter)dataConnection.Command.Parameters["aOutParam"]).Value);

			return ret;
		}

		public static IEnumerable<Person> TestProcedure(this DataConnection dataConnection, int? param3, ref int? param2, out int? param1)
		{
			var ret = dataConnection.QueryProc<Person>("`TestProcedure`",
				new DataParameter("param3", param3, DataType.Int32),
				new DataParameter("param2", param2, DataType.Int32) { Direction = ParameterDirection.InputOutput },
				new DataParameter("param1", null, DataType.Int32) { Direction = ParameterDirection.Output }).ToList();

			param2 = Converter.ChangeTypeTo<int?>(((IDbDataParameter)dataConnection.Command.Parameters["param2"]).Value);
			param1 = Converter.ChangeTypeTo<int?>(((IDbDataParameter)dataConnection.Command.Parameters["param1"]).Value);

			return ret;
		}
	}
}
