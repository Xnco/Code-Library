using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterPattern
{
    class Program
    {
        static void Main(string[] args)
        {
            TargetClass target = new AdapterClass();

            // 用 适配器类 的实例调用的是 某一个不相关的类的某个方法 -> 
            // 一般是一个现有的类，接口不是很符合要求，用适配器去进行扩展
            // 说白了就是一个类封装了一个另一个无关类的实例
            target.Request();
        }
    }

    // 要用的目标类
    class TargetClass
    {
        public virtual void Request()
        {
            Console.WriteLine("Called Target Request()!");
        }
    }

    // 适配器类 - 看起来是调用Target的接口，但实际上调用的是Adaptee的接口
    class AdapterClass : TargetClass
    {
        
        private Adaptee adaptee = new Adaptee();
        public override void Request()
        {
            // 调用的是 适配者的 请求方法
            adaptee.SpecificRequest();
        }
    }

    // 适配者
    class Adaptee
    {
        public void SpecificRequest()
        {
            Console.WriteLine("Called SpecificRequest()");
        }
    }

    /* Q/A: 
     1. Q: 为什么 Adaptee(适配者) 不直接继承 Target(目标)？
        A：Adaptee(适配者本身) 也有可能要继承其他类，而 C# 不能继承多个类
    */

    // 案例2

    // 媒体播放器接口 - Target(目标)接口
    public interface MediaPlayer
    {
        void Play(string audioType, string fileName);
    }

    // 高级媒体播放器接口
    public interface AdvancedMediaPlayer
    {
        void PlayVlc(string fileName);
        void PlayMp4(string fileName);
    }

    // Vlc播放器 - 只实现一个功能 - 对普通播放器的扩展
    public class VlcPlayer : AdvancedMediaPlayer
    {
        public void PlayVlc(string fileName)
        {
            Console.WriteLine("Playing vlc file. Name: " + fileName);
        }

        public void PlayMp4(string fileName)
        {
            // Do nothing
        }
    }

    // Mp4播放器 - 只实现一个功能 - 对普通播放器的扩展
    public class Mp4Player : AdvancedMediaPlayer
    {
        public void PlayVlc(String fileName)
        {
            // Do nothing
        }

        public void PlayMp4(String fileName)
        {
            Console.WriteLine("Playing mp4 file. Name: " + fileName);
        }
    }

    // 适配器
    public class MediaAdapter : MediaPlayer
    {
        AdvancedMediaPlayer advancedMusicPlayer;

        public MediaAdapter(string audioType)
        {
            if (audioType == "vlc")
            {
                advancedMusicPlayer = new VlcPlayer();
            }
            else if (audioType == "mp4")
            {
                advancedMusicPlayer = new Mp4Player();
            }
        }

        public void Play(string audioType, string fileName)
        {
            if (audioType == "vlc")
            {
                advancedMusicPlayer.PlayVlc(fileName);
            }
            else if (audioType == "mp4")
            {
                advancedMusicPlayer.PlayVlc(fileName);
            }
        }
    }

    // 目标类 - 本身只继承了 MediaPlayer 接口，看起来只能播放 Mp3
    public class AudioPlayer : MediaPlayer
    {
        MediaAdapter mediaAdapter;

        public void Play(string audioType, string fileName)
        {
            if (audioType == "mp3")
            {
                Console.WriteLine("Playing mp3 file. Name: " + fileName);
            }
            else if (audioType == "mp4" || audioType == "vlc")
            {
                // 要播放 mp4和vlc时， 自己没这个功能
                mediaAdapter = new MediaAdapter(audioType);
                mediaAdapter.Play(audioType, fileName);
            }
            else
            {
                Console.WriteLine("Invalid media. " + audioType + " format not supported");
            }
        }
    }
}
