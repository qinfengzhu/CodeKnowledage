### Graph 结构与辅助类

1. `GraphNode`(图节点),`IVertex` (图抽象顶点接口)

```
public interface IVertex
{
    //邻接关系
    IVertex[] Adjacencies { get; }
}
```

```
[Serializable] //与MarshalByRefObject搭配
public class GraphNode:MarshalByRefObject,IVertex
{
    private List<GraphNode> incoming; //入度-->此顶点依赖的集合顶点
    private List<GraphNode> outgoing; //出度-->依赖此顶点的集合顶点

    public GraphNode(){}

    public IVertex[] Adjacencies
    {
        get { return Dependents; }
    }

    //依赖此顶点的顶点
    public GraphNode[] Dependents
    {
        get{
            if(outgoing == null) return new GraphNode[0];
            return outgoing.ToArray();
        }
    }

    //添加依赖此顶点的顶点
    public void AddDependent(GraphNode node)
    {
        Outgoing.Add(node);
        node.Incoming.Add(this);
    }

    //出度
    private List<GraphNode> Outgoing
    {
        get{
            if(outgoing == null) outgoing = new List<GraphNode>();
            return outgoing;
        }
    }
    //入度
    private List<GraphNode> Incoming
    {
        get{
            if(incoming == null) incoming = new List<GraphNode>();
            return incoming;
        }
    }

    //此顶点依赖的顶点
    public GraphNode[] Dependers
    {
        get{
            if(incoming == null) return new GraphNode[0];
            return incoming.ToArray();
        }
    }

    //移除此顶点依赖的顶点
    public void RemoveDepender(GraphNode depender)
    {
        Incoming.Remove(depender);
        depender.RemoveDependent(this);
    }

    //移除依赖此顶点的顶点
    private void RemoveDependent(GraphNode graphNode)
    {
        Outgoing.Remove(graphNode);
    }
}
```

2. `VertexColor` 顶点访问状态, `ColorSet` 设置顶点访问状态功能类,`TimestampSet` 统计顶点被访问次数

```
internal enum VertexColor
{
    NotInThisSet, //不在队列中
    White, //顶点还没有被访问
    Gray,  //顶点正在被访问
    Black  //顶点已经被访问
}
```

```
//表示对象的集合(是唯一的)并且给对象着色
internal class ColorsSet
{
    //存放顶点与访问状态的集合
    private IDictionary<IVertex,VertexColor> items = new Dictionary<IVertex,VertexColor>();

    //初始化的时候所有顶点都为未访问状态
    public ColorsSet(IVertex[] items)
    {
        foreach(IVertex item in items)
        {
            Set(item,VertexColor.White);
        }
    }

    //设置顶点的颜色
    public void Set(IVertex item,VertexColor color)
    {
        items[item] = color;
    }

    //查询顶点的访问状态
    public VertexColor ColorOf(IVertex item)
    {
        if(!item.ContainsKey(item)) return VertexColor.NotInThisSet;
        return (VertexColor) items[item];
    }
}
```

```
//保存顶点的次数
internal class TimestampSet
{
    //顶点与次数集合
    private IDictionary<IVertex,int> items = new Dictionary<IVertex,int>();

    public TimestampSet(){}
    //注册顶点与次数
    public void Register(IVertex item,int time)
    {
        items[item] = time;
    }
    //查询顶点的次数
    public int TimeOf(IVertex item)
    {
        return (int)items[item];
    }
}
```
