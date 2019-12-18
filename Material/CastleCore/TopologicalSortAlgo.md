### 抽象的拓扑排序类-TopologicalSortAlgo

```
//拓扑排序算法
public abstract class TopologicalSortAlgo
{
    public static IVertex[] Sort(IVertex[] graphNodes)
    {
        ColorsSet colors = new ColorsSet(graphNodes); //统一设置为White
        TimestampSet discovery = new TimestampSet();  //发现链
        TimestampSet finish = new TimestampSet();     //最终链
        LinkedList<IVertex> list = new LinkedList<IVertex>(); //拓扑排序后的顶点链表

        int time = 0;//初始顺序标记
        foreach(IVertex node in graphNodes)
        {
            if(colors.ColorOf(node) == VertexColor.White)
            {
                Visit(node,colors,discovery,finish,list,ref time);
            }
        }
        IVertex[] vertices = new IVertex[list.Count];
        list.CopyTo(vertices,0);
        return vertices;
    }

    private static void Visit(IVertex node,ColorsSet colors,TimestampSet discovery,
        TimestampSet finish,LinkedList<IVertex> list,ref int time)
    {
        colors.Set(node,VertexColor.Gray); //标记节点正则被访问
        discovery.Register(node,time++);//注册一个顶点并且附带上顺序
        foreach(IVertex child in node.Adjacencies) ////Adjacencies就是Outgoing的出度有向边的另一个节点
        {
            if(colors.ColorOf(child) == VertexColor.White) //仅当节点不是正被访问、已经被访问完的状态时候
            {
                Visit(child,colors,discovery,finish,list,ref time);
            }
        }
        finish.Register(node,time++);//注册一个顶点并且附带上顺序
        list.AddFirst(node);
        colors.Set(node,VertexColor.Black);
    }
}
```
