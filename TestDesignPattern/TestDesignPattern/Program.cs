using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDesignPattern
{
    class Program
    {
        static void Main(string[] args)
        {
            // 管理员
            Director director = new Director();

            // 产品由不同的创造者创造，构造的内容不同
            Builder builderA = new ConcreteBuilderA();
            director.Construct(builderA); // 管理员 根据不同生产者有不同的构建过程
            Product p1 = builderA.GetResult();

            Builder builderB = new ConcreteBuilderB();
            director.Construct(builderB);
            Product p2 = builderB.GetResult();

            Console.WriteLine(p1);
            Console.WriteLine(p2);
        }
    }

    // 核心管理员(导演)类
    public class Director
    {
        // Builder uses a complex series of steps
        public void Construct(Builder builder)
        {
            builder.BuildPartA();
            builder.BuildPartB();
        }
    }

    public abstract class Builder
    {
        // 构建产品 A 部分
        public abstract void BuildPartA();
        // 构建产品 B 部分
        public abstract void BuildPartB();
        // 获取产品的方法
        public abstract Product GetResult(); 
    }

    // 具体的生成者
    public class ConcreteBuilderA : Builder
    {
        private Product product = new Product();
        public override void BuildPartA()
        {
            product.Add("PartA");
        }
        public override void BuildPartB()
        {
            product.Add("PartB");
        }
        public override Product GetResult()
        {
            return product;
        }
    }

    // 具体的生成者
    public class ConcreteBuilderB : Builder
    {
        private Product product = new Product();
        public override void BuildPartA()
        {
            product.Add("PartX");
        }
        public override void BuildPartB()
        {
            product.Add("PartY");
        }
        public override Product GetResult()
        {
            return product;
        }
    }

    // 产品类
    public class Product
    {
        List<object> parts = new List<object>();

        public void Add(string part)
        {
            parts.Add(part);
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            foreach (var item in parts)
            {
                str.Append(item.ToString());
            }
            return str.ToString();
        }
    }
}
