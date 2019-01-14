using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandPattern
{
    class Program
    {
        static void Main(string[] args)
        {
            Stock foodStock = new Stock();
            // 这两个命令都是针对 foodStock 的
            BuyStock buyCom = new BuyStock(foodStock);
            SellStock sellCom = new SellStock(foodStock);

            Broker broker = new Broker();
            // 命令添加到 执行列表 中去
            broker.TakeOrder(buyCom);
            broker.TakeOrder(sellCom);

            broker.Do();
        }
    }

    // 通过调用者调用接受者执行命令，顺序：调用者→接受者→命令。 
    // 不直接做某件事情，通过 Order(命令) 做某件事，在中间层的地方可以将命令信息记录下来
    // 之后就可以 Redo(重做) Undo(撤销) - 类似象棋的悔棋功能

    // 命令的接口
    public interface Order
    {
        void Execute(); // 执行
    }

    // 仓库购买命令
    public class BuyStock : Order
    {
        private Stock foodStock;

        public BuyStock(Stock stock)
        {
            foodStock = stock;
        }

        // 执行
        public void Execute()
        {
            foodStock.Buy();
        }
    }

    // 仓库卖出命令
    public class SellStock : Order
    {
        private Stock foodStock;

        public SellStock(Stock stock)
        {
            foodStock = stock;
        }

        public void Execute()
        {
            foodStock.Sell();
        }
    }

    // 仓库 - 请求类
    public class Stock
    {
        private string name = "Food";
        private int quantity = 10;

        public void Buy()
        {
            Console.WriteLine("Stock [ Name: " + name + ", Quantity: " + quantity +" ] bought");
        }

        public void Sell()
        {
            Console.WriteLine("Stock [ Name: " + name + ",  Quantity: " + quantity +" ] sold");
        }
    }

    public class Broker
    {
        private List<Order> orderList = new List<Order>(); // 所有命令

        // 添加命令
        public void TakeOrder(Order order)
        {
            orderList.Add(order);
        }

        // 执行命令
        public void Do()
        {
            foreach (var item in orderList)
            {
                // 执行每个命令
                item.Execute();
            }
            orderList.Clear(); // 清空命令列表
        }
    }
}
