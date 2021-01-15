using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Threading;

using ClassMove;


namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DispatcherTimer gameTimer = new DispatcherTimer();
        bool moveLeft, moveRight;
        List<Rectangle> itemRemover = new List<Rectangle>();
        Random rand = new Random();

        int enemySpriteCounter; // int поможет изменить изображения врагов
        int enemyCounter = 100; // Время появления врага
        int playerSpeed = 10; // Скорость передвижения игрока
        int limit = 50; // лимит вражеских икр
        int score = 0; // оценка по умолчанию
        int damage = 0; // повреждение по умолчанию

        Rect playerHitBox; // player hit box для проверки на столкновение с врагом

        public MainWindow()
        {
          
            InitializeComponent();

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);

            gameTimer.Tick += GameLoop;

            gameTimer.Start();

            MyCanvas.Focus();

            ImageBrush bg = new ImageBrush();

            bg.ImageSource = new BitmapImage(new Uri("C:/Users/User/source/repos/WpfApp3/WpfApp3/images/back.png"));

            bg.TileMode = TileMode.Tile;

            //bg.Viewport = new Rect(0, 0, 0.15, 0.15);
            // установите блок порта BG view               можно убрать верхнюю и нижнюю 
            //bg.ViewportUnits = BrushMappingMode.RelativeToBoundingBox;
            // назначить bg в качестве фона моего холста
            MyCanvas.Background = bg;

            ImageBrush playerImage = new ImageBrush();
            playerImage.ImageSource = new BitmapImage(new Uri("C:/Users/User/source/repos/WpfApp3/WpfApp3/images/player.png"));
            // присвоить плееру прямоугольную заливку
            player.Fill = playerImage;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // связать поле попадания игрока с прямоугольником игрока
            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);

            // уменьшить единицу от целого числа счетчика противника
            enemyCounter--;

            scoreText.Content =+ score;  // связать текст оценки с целым числом баллов
            damageText.Content =+ damage; // связать текст повреждения с целым числом повреждения

            // если счетчик противника меньше 0
            if (enemyCounter < 0)
            {
                MakeEnemies();
                enemyCounter = limit; // сбросить счетчик противника до предельного целого числаr
            }

            // начало движение игрока

            if (moveLeft && Canvas.GetLeft(player) > 0)
            {
                // если перемещение влево верно и игрок находится внутри границы, то переместите игрока влево
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            }
            if (moveRight && Canvas.GetLeft(player) + 90 < Application.Current.MainWindow.Width)
            {
                // если ход вправо истинен, а игрок влево + 90 меньше ширины формы
                // затем переместите игрока вправо
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            }

            // движение игрока заканчивается

            // поиск пуль, врагов и столкновений начинается

            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                // если в каком-либо прямоугольнике есть маркер тега
                if (x is Rectangle && (string)x.Tag == "bullet")
                {
                    // переместить прямоугольник пули в верхнюю часть экрана
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20);

                    // создать класс rect со свойствами bullet rectangles
                    Rect bullet = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    // проверить, достигла ли пуля верхней части экрана
                    if (Canvas.GetTop(x) < 10)
                    {
                        // если так, то добавьте его в список элементов для удаления
                        itemRemover.Add(x);
                    }

                    // запустить еще один для каждого цикла внутри основного цикла этот цикл имеет локальную переменную под названием y
                    foreach (var y in MyCanvas.Children.OfType<Rectangle>())
                    {
                        // если y-это прямоугольник и у него есть тег под названием enemy
                        if (y is Rectangle && (string)y.Tag == "enemy")
                        {
                            // создать локальную rect called под названием enemy и поместите в нее свойства enemies
                            Rect enemy = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);


                            // теперь проверим, сталкиваются ли пуля и враг или нет
                            // если пуля сталкивается с прямоугольником противника
                            if (bullet.IntersectsWith(enemy))
                            {

                                itemRemover.Add(x); // удалить пулю
                                itemRemover.Add(y); // удалить врага
                                score++; // увеличиваем счетчик 
                            }
                        }

                    }
                }

                // за пределами второй петли  снова проверим, нет ли врага
                if (x is Rectangle && (string)x.Tag == "enemy")
                {
                    // если мы найдем прямоугольник с меткой врага

                    Canvas.SetTop(x, Canvas.GetTop(x) + 10); // move the enemy downwards

                    Rect enemy = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);

                    // сначала проверьте, прошел ли вражеский объект мимо игрока.
                    // its gone прошел 700 пикселей сверху
                    if (Canvas.GetTop(x) + 150 > 700)
                    {
                        // если да, то сначала удалите вражеский объект
                        itemRemover.Add(x);
                        damage += 10; // добовляем 10 к урону
                    }

                    // если игрок попал в бокс и противник столкнулся 
                    if (playerHitBox.IntersectsWith(enemy))
                    {
                        damage += 5; // add 5 to the damage
                        itemRemover.Add(x); //  удалить вражеский объект
                    }
                }


            }

            // поиск пуль, врагов и столкновений заканчивается

            // если счет больше 5 
            if (score > 5)
            {
                limit = 20; // уменьшить лимит до 20
                // теперь враги будут появляться быстрее
            }

            // если целое число повреждений больше 99
            if (damage > 99)
            {
                gameTimer.Stop(); // остановить основной таймер
                damageText.Content = "100";
                damageText.Foreground = Brushes.Red; // измените цвет текста на красный
            }

            // удаление прямоугольников

            // проверьте, сколько прямоугольников находится внутри списка элементов для удаления
            foreach (Rectangle y in itemRemover)
            {
                // навсегда удалите их с холста
                MyCanvas.Children.Remove(y);
            }
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {


            if (e.Key == Key.A)
            {
                moveLeft = true;
            }
            if (e.Key == Key.D)
            {
                moveRight = true;
            }

        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                moveLeft = false;
            }
            if (e.Key == Key.D)
            {
                moveRight = false;
            }

            if (e.Key == Key.Space)
            {
                Rectangle newBullet = new Rectangle
                {
                    Tag = "bullet",
                    Height = 20,
                    Width = 5,
                    Fill = Brushes.White,
                    Stroke = Brushes.OrangeRed
                };

                Canvas.SetTop(newBullet, Canvas.GetTop(player) - newBullet.Height);

                Canvas.SetLeft(newBullet, Canvas.GetLeft(player) + player.Width / 2);

                MyCanvas.Children.Add(newBullet);
            }
        }


        private void MakeEnemies()
        {
            ImageBrush enemySprite = new ImageBrush();

            enemySpriteCounter = rand.Next(1, 5);

            switch (enemySpriteCounter)
            {
                case 1:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:/Users/User/source/repos/WpfApp3/WpfApp3/images/1.png"));
                    break;
                case 2:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:/Users/User/source/repos/WpfApp3/WpfApp3/images/2.png"));
                    break;
                case 3:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:/Users/User/source/repos/WpfApp3/WpfApp3/images/3.png"));
                    break;
                case 4:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:/Users/User/source/repos/WpfApp3/WpfApp3/images/4.png"));
                    break;
                case 5:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:/Users/User/source/repos/WpfApp3/WpfApp3/images/5.png"));
                    break;
                default:
                    enemySprite.ImageSource = new BitmapImage(new Uri("C:/Users/User/source/repos/WpfApp3/WpfApp3/images/1.png"));
                    break;
            }

            Rectangle newEnemy = new Rectangle
            {
                Tag = "enemy",
                Height = 50,
                Width = 56,
                Fill = enemySprite
            };

            Canvas.SetTop(newEnemy, -100);

            Canvas.SetLeft(newEnemy, rand.Next(30, 430));

            MyCanvas.Children.Add(newEnemy);

            GC.Collect();  // соберите все неиспользуемые ресурсы для этой игры
        }

    }
}
