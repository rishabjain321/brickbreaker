using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// observable collections
using System.Collections.ObjectModel;

// debug output
using System.Diagnostics;

// timer, sleep
//using System.Threading;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Timers;

// hi res timer
using PrecisionTimers;

// Rectangle
// Must update References manually
using System.Drawing;

// INotifyPropertyChanged
using System.ComponentModel;

namespace BouncingBall
{
    public partial class Model : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private static UInt32 _numBalls = 1;
        private UInt32[] _buttonPresses = new UInt32[_numBalls];
        Random _randomNumber = new Random();
        //private Thread _threadPaddle = null;
        //private Thread _threadBall = null;
        private TimerQueueTimer.WaitOrTimerDelegate _ballTimerCallbackDelegate;
        private TimerQueueTimer.WaitOrTimerDelegate _paddleTimerCallbackDelegate;
        private TimerQueueTimer _ballHiResTimer;
        private TimerQueueTimer _paddleHiResTimer;
        private double _ballXMove = 1;
        private double _ballYMove = 1;
        System.Drawing.Rectangle _ballRectangle;
        System.Drawing.Rectangle _paddleRectangle;
        bool _movepaddleLeft = false;
        bool _movepaddleRight = false;
        private bool _moveBall = false;
        System.Timers.Timer _timer;




        public ObservableCollection<Brick> BrickCollection;
        int _numBricks = 79;
        Rectangle[] _brickRectangles = new Rectangle[4];
        // note that the brick hight, number of brick columns and rows
        // must match our window demensions.
        double _brickHeight = 20;
        double _brickWidth = 50;

        public bool MoveBall
        {
            get { return _moveBall; }
            set { _moveBall = value; }
        }

        private double _windowHeight = 100;
        public double WindowHeight
        {
            get { return _windowHeight; }
            set { _windowHeight = value; }
        }

        private double _windowWidth = 100;
        public double WindowWidth
        {
            get { return _windowWidth; }
            set { _windowWidth = value; }
        }

        /// <summary>
        /// Model constructor
        /// </summary>
        /// <returns></returns>
        public Model()
        {
        }

        public void InitModel()
        {
            //_threadPaddle = new Thread(new ThreadStart(paddleThreadCall));
            //_threadBall = new Thread(new ThreadStart(ballThreadCall));

            //_threadPaddle.Start();
            //_threadBall.Start();
            _ballTimerCallbackDelegate = new TimerQueueTimer.WaitOrTimerDelegate(BallMMTimerCallback);
            _paddleTimerCallbackDelegate = new TimerQueueTimer.WaitOrTimerDelegate(paddleMMTimerCallback);

            Score = 0;
            // create our multi-media timers
            _ballHiResTimer = new TimerQueueTimer();
            try
            {
                // create a Multi Media Hi Res timer.
                _ballHiResTimer.Create(3, 3, _ballTimerCallbackDelegate);
            }
            catch (QueueTimerException ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Failed to create Ball timer. Error from GetLastError = {0}", ex.Error);
            }

            _paddleHiResTimer = new TimerQueueTimer();
            try
            {
                // create a Multi Media Hi Res timer.
                _paddleHiResTimer.Create(2, 2, _paddleTimerCallbackDelegate);
            }
            catch (QueueTimerException ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Failed to create paddle timer. Error from GetLastError = {0}", ex.Error);
            }
        // create brick collection
        // place them manually at the top of the item collection in the view
        BrickCollection = new ObservableCollection<Brick>();
            int i = 0;
            double curLeft = 0;
            for (int brick = 0; brick < _numBricks; brick++)
            {
                BrickCollection.Add(new Brick()
                {
                    BrickHeight = _brickHeight,
                    BrickWidth = _brickWidth,
                    BrickVisible = System.Windows.Visibility.Visible,
                    BrickName = brick.ToString(),
                });

                //for creating the collection
                //{

                if (WindowWidth < curLeft)
                {
                    i++;
                    curLeft = 0;
                }
                BrickCollection[brick].BrickCanvasLeft = curLeft;
                BrickCollection[brick].BrickCanvasTop = i * _brickHeight;

                curLeft += _brickWidth;

                 //}
                // offset the bricks from the top of the screen by a bitg
            }

            UpdateRects();
        
    }

        public void CleanUp()
        {
            /* if (_threadPaddle != null && _threadPaddle.IsAlive)
                 _threadPaddle.Abort();
             if (_threadBall != null && _threadBall.IsAlive)
                 _threadBall.Abort();
                 */
            _ballHiResTimer.Delete();
            _paddleHiResTimer.Delete();
        }


        public void SetStartPosition()
        {
            
            BallHeight = 15;
            BallWidth = 15;
            paddleWidth = 120;
            paddleHeight = 05;
            
            ballCanvasLeft = _windowWidth/2 - BallWidth/2;
            ballCanvasTop = _windowHeight/1.155;
            _moveBall = false;
            
            paddleCanvasLeft = _windowWidth / 2 - paddleWidth / 2;
            paddleCanvasTop = _windowHeight - paddleHeight - 30;
            _paddleRectangle = new System.Drawing.Rectangle((int)paddleCanvasLeft, (int)paddleCanvasTop, (int)paddleWidth, (int)paddleHeight);
        }

        public void MoveLeft(bool move)
        {
            _movepaddleLeft = move;
        }

        public void MoveRight(bool move)
        {
            _movepaddleRight = move;
        }

        public void timer() {
            _timer = new System.Timers.Timer(200);
            _timer.Elapsed  += new ElapsedEventHandler(TimerHandler);
            _timer.Start();

        }


        private void TimerHandler(object source, ElapsedEventArgs e)
        {
            if(_moveBall == true)
                timeElapsed += 1;
               
            
        
                
            
        }

        private void UpdateRects()
        {
            _ballRectangle = new System.Drawing.Rectangle((int)ballCanvasLeft, (int)ballCanvasTop, (int)BallWidth, (int)BallHeight);
            for (int brick = 0; brick < _numBricks; brick++)
                BrickCollection[brick].BrickRectangle = new System.Drawing.Rectangle((int)BrickCollection[brick].BrickCanvasLeft,
                    (int)BrickCollection[brick].BrickCanvasTop, (int)BrickCollection[brick].BrickWidth, (int)BrickCollection[brick].BrickHeight);
        }


        private void BallMMTimerCallback(IntPtr pWhat, bool success)
        //private void ballThreadCall()

        {
           
                if (!_moveBall)
                    return;

                // start executing callback. this ensures we are synched correctly
                // if the form is abruptly closed
                // if this function returns false, we should exit the callback immediately
                // this means we did not get the mutex, and the timer is being deleted.
                if (!_ballHiResTimer.ExecutingCallback())
                {
                    Console.WriteLine("Aborting timer callback.");
                    return;
                }
                
                ballCanvasLeft += _ballXMove;
                ballCanvasTop += _ballYMove;

                // check to see if ball has it the left or right side of the drawing element
                if ((ballCanvasLeft + BallWidth >= _windowWidth) ||
                    (ballCanvasLeft <= 0))
                    _ballXMove = -_ballXMove;


                // check to see if ball has it the top of the drawing element
                if (ballCanvasTop <= 0)
                    _ballYMove = -_ballYMove;

                if (ballCanvasTop + BallWidth >= _windowHeight)
                {
                    // we hit bottom. stop moving the ball
                    _moveBall = false;
                }

                // see if we hit the paddle
                _ballRectangle = new System.Drawing.Rectangle((int)ballCanvasLeft, (int)ballCanvasTop, (int)BallWidth, (int)BallHeight);
                if (_ballRectangle.IntersectsWith(_paddleRectangle))
                {
                    // hit paddle. reverse direction in Y direction
                    _ballYMove = -_ballYMove;

                    // move the ball away from the paddle so we don't intersect next time around and
                    // get stick in a loop where the ball is bouncing repeatedly on the paddle
                    ballCanvasTop += 2 * _ballYMove;

                    // add move the ball in X some small random value so that ball is not traveling in the same 
                    // pattern
                    ballCanvasLeft += _randomNumber.Next(5);
                }

            _ballHiResTimer.DoneExecutingCallback();
            CheckPush();
            
             // done in callback. OK to delete timer
            //_ballHiResTimer.DoneExecutingCallback();
        }

        private void paddleMMTimerCallback(IntPtr pWhat, bool success)
        //private void paddleThreadCall()
        {

            // start executing callback. this ensures we are synched correctly
            // if the form is abruptly closed
            // if this function returns false, we should exit the callback immediately
            // this means we did not get the mutex, and the timer is being deleted.
            if (!_paddleHiResTimer.ExecutingCallback())
            {
                Console.WriteLine("Aborting timer callback.");
                return;
            }
            
            {
                if (_movepaddleLeft && paddleCanvasLeft > 0)
                    paddleCanvasLeft -= 2;
                else if (_movepaddleRight && paddleCanvasLeft < _windowWidth - paddleWidth)
                    paddleCanvasLeft += 2;

                //Thread.Sleep(2);

                _paddleRectangle = new System.Drawing.Rectangle((int)paddleCanvasLeft, (int)paddleCanvasTop, (int)paddleWidth, (int)paddleHeight);
            }
            

            // done in callback. OK to delete timer
            _paddleHiResTimer.DoneExecutingCallback();
        }
        
        private void CheckPush()
        {
            timer();
            for (int brick = 0; brick < _numBricks; brick++)
            {
                if (BrickCollection[brick].BrickVisible  == Visibility.Hidden ) continue;

                InterectSide whichSide = IntersectsAt(BrickCollection[brick].BrickRectangle, _ballRectangle);
                switch (whichSide)
                {
                    case InterectSide.NONE:
                        break;

                    case InterectSide.TOP:
                        BrickCollection[brick].BrickVisible = Visibility.Hidden;
                        _ballYMove = -_ballYMove;
                        ballCanvasTop += 2 * _ballYMove;
                        ballCanvasLeft += _randomNumber.Next(5);
                        Score++;

                        break;

                    case InterectSide.BOTTOM:
                        BrickCollection[brick].BrickVisible = Visibility.Hidden;
                        _ballYMove = -_ballYMove;
                        ballCanvasTop += 2 * _ballYMove;
                        ballCanvasLeft += _randomNumber.Next(5);
                        Score++;
                        break;

                    case InterectSide.LEFT:
                        BrickCollection[brick].BrickVisible = Visibility.Hidden;
                        _ballXMove = -_ballXMove;
                        ballCanvasTop += 2 * _ballXMove;
                        ballCanvasLeft += _randomNumber.Next(5);
                        Score++;
                        break;

                    case InterectSide.RIGHT:
                        BrickCollection[brick].BrickVisible = Visibility.Hidden;
                        _ballXMove = -_ballXMove;
                        ballCanvasTop += 2 * _ballXMove;
                        ballCanvasLeft += _randomNumber.Next(5);
                        Score++;
                        break;
                }
            }
        }

        enum InterectSide { NONE, LEFT, RIGHT, TOP, BOTTOM };
        private InterectSide IntersectsAt(Rectangle brick, Rectangle ball)
        {
            if (brick.IntersectsWith(ball) == false)
                return InterectSide.NONE;

            Rectangle r = Rectangle.Intersect(brick, ball);

            // did we hit the top of the brick
            if (ball.Top + ball.Height - 1 == r.Top &&
                r.Height == 1)
                return InterectSide.TOP;

            if (ball.Top == r.Top &&
                r.Height == 1)
                return InterectSide.BOTTOM;

            if (ball.Left == r.Left &&
                r.Width == 1)
                return InterectSide.RIGHT;

            if (ball.Left + ball.Width - 1 == r.Left &&
                r.Width == 1)
                return InterectSide.LEFT;

            return InterectSide.NONE;
        }

    }
}
