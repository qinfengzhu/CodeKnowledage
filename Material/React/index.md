### React 学习资料

1. 深入React技术栈 pdf
2. [React.js小书](http://huziketang.mangojuice.top/books/react/lesson1)

### React 学习步骤

1. 搭建简单的React测试环境

```
<!DOCTYPE html>
<html>
<head>
<meta charset="UTF-8" />
<title>React 实例</title>
<script src="https://cdn.staticfile.org/react/16.4.0/umd/react.development.js"></script>
<script src="https://cdn.staticfile.org/react-dom/16.4.0/umd/react-dom.development.js"></script>
<script src="https://cdn.staticfile.org/babel-standalone/6.26.0/babel.min.js"></script>
</head>
<body>
<div id="example"></div>
<script type="text/babel">
class MyComponent extends React.Component {
  handleClick=(e)=> {
    //使用原生的 DOM API 给最新的值
    this.refs.myInput.value='点击改变值';
  }
  render() {
    //  当组件插入到 DOM 后，ref 属性添加一个组件的引用于到 this.refs
    return (
      <div>
        <input type="text" ref="myInput" />
        <input
          type="button"
          value="点我输入框获取焦点"
          onClick={this.handleClick}
        />
      </div>
    );
  }
}
ReactDOM.render(
  <MyComponent />,
  document.getElementById('example')
);
</script>
</body>
</html>
```
