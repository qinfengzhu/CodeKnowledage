### 快速搭建索引存储

1. 创建索引写入器(Creating an IndexWriter)

```
import org.apache.lucene.index.IndexWriter;
import org.apache.lucene.store.Directory;
import org.apache.lucene.analysis.standard.StandardAnalyzer;
...
private IndexWriter writer;
...
public Indexer(string indexDir) throws IOException{
  Directory dir = FSDirectory.open(new File(indexDir));
  writer = new IndexWriter(dir,
                           new StandardAnalyzer(Version.LUCENE_30),
                           true,
                           IndexWriter.MaxFieldLength.UNLIMITED);
}
```

2. 创建一个文档包含一些字段(A Document contains Fields)

```
import org.apache.lucene.document.Document;
import org.apache.lucene.document.Field;
...
protected Document getDocument(File f) throws Exception{
  Document doc = new Document();
  doc.add(new Field("contents",new FileReader(f)));
  doc.add(new Field("filename",f.getName(),Field.Store.YES,Field.Index.NOT_ANALYZED));
  doc.add(new Field("fullpath",f.getCanonicalPath(),Field.Store.YES,Field.Index.NOT_ANALYZED));
  return doc;
}
```

3. 使用`IndexWriter` 为一个文档构建索引(Index a Document with IndexWriter)

```
private IndexWriter writer;
...
private void indexFile(File f) throws Exception{
  Document doc = getDocument(f);
  writer.addDocument(doc);
}
```

4. 为一个文件夹构建索引(Indexing a directory)

```
private IndexWriter writer;
...
public int index(string dataDir,FileFilter filter) throws Exception{
  File[] files = new File(dataDir).listFiles();
  for(File f:files){
    if(...&&(filter==null||filter.accept(f))){
      indexFile(f);
    }
  }
  return writer.numDocs();
}
```

5. 关闭索引写入器(Closing the IndexWriter)

```
private IndexWriter writer;
...
public void close() throws IOException{
  writer.close();
}
```

### 快速搭建索引查询

1. 创建索引查询器(Creating an IndexSearcher)

```
import org.apache.lucene.search.IndexSearcher;
...
public static void search(string indexDir,string q) throw IOException,ParseException{
  Directory dir = FSDirectory.open(new File(indexDir));
  indexSearcher is = new IndexSearcher(dir);
  ...
}
```

2. 创建查询以及查询转换器(Query and QueryParser)

```
import org.apache.lucene.search.Query;
import org.apache.lucene.queryParser.QueryParser;
...
public static void search(string indexDir,string q) throws IOException,ParseException{
  ...
  QueryParser parser = new QueryParser(Version.LUCENE_30,"contents",new StandardAnalyzer(Version.LUCENE_30));
  Query query = parser.parse(q);
}
```

3. 查询并且返回置顶文档 (search() returns TopDocs)

```
import org.apache.lucene.search.TopDocs;
...
public static void search(string indexDir,string q) throws IOException,ParseException{
  ...
  IndexSearcher is = ...;
  ...
  Query query = ...;
  ...
  TopDocs hits = is.search(query,10);
}
```

4. 置顶文档是否包含计分文档 (TopDocs contain ScoreDocs)

```
import org.apache.lucene.search.ScoreDoc;
...
public static void search(string indexDir,string q) throws IOException,ParseException{
  ...
  IndexSearcher is = ...;
  ...
  TopDocs hits = ...;
  ...
  for(ScoreDoc scoreDoc : hits.scoreDocs){
    Document doc = is.doc(scoreDoc.doc);
    System.out.println(doc.get("fullpath"));
  }
}
```

5. 关闭索引查询器 (Closing IndexSearcher)

```
public static void search(string idnexDir,string q) throws IOException,ParseException{
  ...
  IndexSearcher is = ...;
  ...
  is.close();
}
```
