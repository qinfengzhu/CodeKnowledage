#### 使用Mybatis写Dao层

1. `com.bestkf.dao` 包中定义

```
public interface AdminMapper
{
    //验证登录信息是否正确
    Admin login(LoginForm loginForm);

    //通过姓名查询指定管理员信息
    Admin findByName(String name);

    //添加管理员信息
    int insert(Admin admin);

    //根据姓名查询指定/所有管理员信息列表
    List<Admin> selectList(Admin admin);

    //根据id更新指定管理员信息
    int update(Admin admin);

    //根据id修改指定管理员密码
    int updatePassword(Admin admin);

    //根据id删除指定管理员信息
    int deleteById(Integer[] ids);
}
```

2. 对应的 `Mapper`文件

```
<?xml version="1.0" encoding="UTF-8" ?>
<!DOCTYPE mapper PUBLIC "-//mybatis.org//DTD Mapper 3.0//EN" "http://mybatis.org/dtd/mybatis-3-mapper.dtd" >
<!--这里特别注意namespace 是对应Mapper类的包路径-->
<mapper namespace="com.bestkf.dao.AdminMapper">  
    <!-- 验证登录信息是否正确 -->
    <select id="login" parameterType="loginForm" resultType="admin">
        SELECT id,
               name,
               gender,
               password,
               email,
               telephone,
               address,
               portrait_path
        FROM tb_admin
        WHERE name = #{username}
          AND password = #{password}
    </select>

    <!-- 根据id查询指定管理员信息 -->
    <select id="findByName" parameterType="String" resultType="admin">
        SELECT id,
               name,
               gender,
               password,
               email,
               telephone,
               address,
               portrait_path
        FROM tb_admin
        WHERE name = #{name}
    </select>

    <!-- 添加管理员信息 -->
    <insert id="insert" parameterType="admin">
        INSERT INTO tb_admin(name, gender, password, email, telephone, address, portrait_path)
        VALUES (#{name}, #{gender}, #{password}, #{email}, #{telephone}, #{address}, #{portrait_path})
    </insert>

    <!-- 根据姓名模糊查询指定/所有管理员信息 列表 -->
    <select id="selectList" parameterType="admin" resultType="admin">
        SELECT id, name, gender, password, email, telephone, address, portrait_path
        FROM tb_admin
        <where>
            <!-- name为Admin中的属性名(在Controller层中已将查询的姓名封装到了Admin中的name属性中) -->
            <if test="name!=null and name!=''">
                AND name LIKE concat(concat('%',#{name}),'%')
            </if>
        </where>
    </select>

    <!-- 根据id更新指定管理员信息 -->
    <update id="update" parameterType="admin">
        UPDATE tb_admin
        <set>
            <if test="name!=null and name!=''">name=#{name},</if>
            <if test="gender!=null and gender!=''">gender=#{gender},</if>
            <if test="email!=null and email!=''">email=#{email},</if>
            <if test="telephone!=null and telephone!=''">telephone=#{telephone},</if>
            <if test="address!=null and address!=''">address=#{address},</if>
            <if test="portrait_path!=null and portrait_path!=''">portrait_path=#{portrait_path},</if>
        </set>
        WHERE id = #{id}
    </update>

    <!-- 根据id修改指定用户密码 -->
    <update id="updatePassword" parameterType="admin">
        UPDATE tb_admin
        SET password = #{password}
        WHERE id = #{id}
    </update>

    <!-- 根据id批量删除管理员信息 -->
    <delete id="deleteById">
        DELETE FROM ssm_sms.tb_admin WHERE id IN
        <foreach collection="array" item="ids" open="(" separator="," close=")">
            #{ids}
        </foreach>
    </delete>
</mapper>
```

3. 对应的 `mybatis` 配置

3.1 `applicationContext.xml` Spring的配置文件中

```
<!-- 读取c3p0.properties中的数据库配置信息 -->
<context:property-placeholder location="classpath:c3p0.properties"/>

<!-- 配置数据源 -->
<bean id="dataSource" class="com.mchange.v2.c3p0.ComboPooledDataSource">
    <!-- 数据库驱动 -->
    <property name="driverClass" value="${c3p0.driverClass}"/>
    <!-- 连接数据库的url -->
    <property name="jdbcUrl" value="${c3p0.jdbcUrl}"/>
    <!-- 连接数据库的用户名 -->
    <property name="user" value="${c3p0.user}"/>
    <!-- 连接数据库的密码 -->
    <property name="password" value="${c3p0.password}"/>
    <!-- 初始化连接数 -->
    <property name="initialPoolSize" value="${c3p0.initialPoolSize}"/>
    <!-- 最大连接数 -->
    <property name="maxPoolSize" value="${c3p0.maxPoolSize}"/>
    <!-- 最小连接数 -->
    <property name="minPoolSize" value="${c3p0.minPoolSize}"/>
    <!-- 连接的生存时间 -->
    <property name="maxIdleTime" value="${c3p0.maxIdleTime}"/>
</bean>
<!-- MyBatis与Spring整合 -->
<bean id="sqlSessionFactory" class="org.mybatis.spring.SqlSessionFactoryBean">
    <!-- 注入数据源 -->
    <property name="dataSource" ref="dataSource"/>
    <!-- 指定Mapper映射文件位置 -->
    <property name="mapperLocations" value="classpath:mappers/*.xml"/>
    <!-- 指定MyBatis核心配置文件位置 -->
    <property name="configLocation" value="classpath:mybatis-config.xml"/>
    <!-- 引入插件 -->
    <property name="plugins">
        <array>
            <!-- 引入MyBaits分页插件 -->
            <bean class="com.github.pagehelper.PageInterceptor">
                <property name="properties">
                    <!-- 指定数据库类型 -->
                    <value>helperDialct=mysql</value>
                </property>
            </bean>
        </array>
    </property>
</bean>

<!-- 开启Mapper接口扫描器: 扫描Dao层,这个是 mybatis-spring包中的-->
<bean class="org.mybatis.spring.mapper.MapperScannerConfigurer">
    <property name="basePackage" value="com.bestkf.dao"/>
</bean>

<!-- 配置Spring事务管理器 -->
<bean id="transactionManager" class="org.springframework.jdbc.datasource.DataSourceTransactionManager">
    <!-- 原理:控制数据源 -->
    <property name="dataSource" ref="dataSource"/>
</bean>
<!-- 开启Spring IOC注解扫描器: 扫描Servie层-->
<context:component-scan base-package="com.bestkf.service"/>
```
3.2 `mybatis-config.xml` 文件

```
<?xml version="1.0" encoding="UTF-8" ?>
<!DOCTYPE configuration PUBLIC "-//mybatis.org//DTD Config 3.0//EN" "http://mybatis.org/dtd/mybatis-3-config.dtd">
<configuration>
    <typeAliases>
        <package name="pers.huangyuhui.sms.bean"/>
    </typeAliases>
</configuration>
```

3.3 `Service`层接口以及实现 (在`applicationContext.xml`)文件中已经添加了对`Servie`包的扫描


3.3.1 `AdminService`接口

```
package com.bestkf.service;
public interface AdminService
{
    //验证登录信息是否正确
    Admin login(LoginForm loginForm);

    //根据用户名查询指定管理员信息
    Admin findByName(String name);

    //添加管理员信息
    int insert(Admin admin);

    //根据姓名查询指定/所有管理员信息列表
    List<Admin> selectList(Admin admin);

    //根据id更新指定管理员信息
    int update(Admin admin);

    //根据id修改指定用户密码
    int updatePassowrd(Admin admin);

    //根据id删除管理员信息
    int deleteById(Integer[] ids);
}
```

3.3.2 `AdminService` 接口的实现 `AdminServiceImpl`

```
@Service   //标记Service,让Spring 容器可以扫描包来添加它
@Transactional //添加事务
public class AdminServiceImpl implements AdminService
{
    //自动解析
    @Autowired  
    private AdminMapper adminMapper;

    @Override
    public Admin login(LoginForm loginForm) { return adminMapper.login(loginForm); }

    @Override
    public List<Admin> selectList(Admin admin) {
        return adminMapper.selectList(admin);
    }

    @Override
    public Admin findByName(String name) {
        return adminMapper.findByName(name);
    }

    @Override
    public int insert(Admin admin) {
        return adminMapper.insert(admin);
    }

    @Override
    public int update(Admin admin) { return adminMapper.update(admin); }

    @Override
    public int updatePassowrd(Admin admin) {
        return adminMapper.updatePassword(admin);
    }

    @Override
    public int deleteById(Integer[] ids) {
        return adminMapper.deleteById(ids);
    }
}
```
