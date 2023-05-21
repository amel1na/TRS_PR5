using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Amelina_App5
{
    internal class Program
    {
        static SemaphoreSlim _bufferLock = new SemaphoreSlim(1); // Семафор для доступу до буфера
        static SemaphoreSlim _itemsAvailable = new SemaphoreSlim(0); // Семафор для доступних елементів в буфері
        static SemaphoreSlim _spaceAvailable; // Семафор для доступного місця в буфері

        static int _bufferSize; // Розмір буфера
        static int _itemsToConsume; // Кількість елементів, які споживач має зчитати


        static void Main()
        {
            _bufferSize = 5;
            _itemsToConsume = 10;

            _spaceAvailable = new SemaphoreSlim(_bufferSize); // Ініціалізуємо семафор для доступного місця в буфері

            Producer[] producers = new Producer[3]; // Створюємо 3 виробника
            Consumer consumer = new Consumer(); // Створюємо споживач

            for (int i = 0; i < producers.Length; i++)
            {
                producers[i] = new Producer(i + 1); // Встановлюємо індекс виробника
                Thread producerThread = new Thread(producers[i].Produce);
                producerThread.Start();
            }

            Thread consumerThread = new Thread(consumer.Consume);
            consumerThread.Start();
        }
        /// <summary>
        /// Виробник
        /// </summary>
        class Producer
        {
            private int _id;

            public Producer(int id)
            {
                _id = id;
            }

            public void Produce()
            {
                for (int i = 0; i < _itemsToConsume; i++)
                {
                    _spaceAvailable.Wait(); // Зачекати на доступне місце в буфері

                    _bufferLock.Wait(); // Заблокувати буфер

                    Console.WriteLine($"Producer {_id} produced item {i + 1}");
                    // Додати елемент до буфера

                    _bufferLock.Release(); // Розблокувати буфер

                    _itemsAvailable.Release(); // Звільнити семафор доступних елементів
                }
            }
        }
         /// <summary>
         /// Споживач 
         /// </summary>
        class Consumer
        {
            public void Consume()
            {
                for (int i = 0; i < _itemsToConsume; i++)
                {
                    _itemsAvailable.Wait(); // Зачекати на доступний елемент

                    _bufferLock.Wait(); // Заблокувати буфер

                    // Зчитати елемент з буфера
                    Console.WriteLine($"Consumer consumed item {i + 1}");

                    _bufferLock.Release(); // Розблокувати буфер

                    _spaceAvailable.Release(); // Звільнити семафор доступного місця
                }
            }
        }
    }
}
