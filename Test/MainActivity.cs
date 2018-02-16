using Android.App;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Media;
using System.IO;
using System;
using Android.Content;
using static Test.Config.ProgramConstants;

//TODO: Захват растояния в реальном времени с камеры, отрисовка прямоугольника 
//при фиксировании лица
//Сохранение фото
//Применение фрагментов, вместо переключения Activity

namespace Test
{
    [Activity(Label = "Face detection v1.1B", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        //Получил Bitmap из картинки
        private Bitmap cameraBitmap = null;
        //Intent intent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Установка основного экрана
            SetContentView(Resource.Layout.Main);
            //Событие на кнопку для кнопки сделать фото;
            ((Button)FindViewById(Resource.Id.take_picture)).Click += btntake_HandleClick;
        }

        #region Button Handlers
        //Обработчик для открытия камеры
        void btntake_HandleClick(object sender, EventArgs e)
        {
            //Вызов события OpenCamera() 
            OpenCamera();
        }

        //Обработчик кнопки определения лиц
        void btnDetect_HandleClick(object sender, EventArgs e)
        {
            //Вызываем метод определения лиц
            detectFaces();
        }

        //Обработчик кнопки назад
        void btnBack_HandleClick(object sender, EventArgs e)
        {
            FinishActivity(Resource.Layout.detectlayout);
            SetContentView(Resource.Layout.Main);
            //Событие на кнопку для кнопки сделать фото;
            ((Button)FindViewById(Resource.Id.take_picture)).Click += btntake_HandleClick;
        }
        #endregion

              
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            //Проверяем запрос кода с константой
            if (TAKE_PICTURE_CODE == requestCode)
            {
                //Запускаем процесс обработки данных изображения камеры
                ProcessCameraImage(data);
            }
        }

        /// <summary>
        /// Метод для открытия камеры
        /// </summary>
        private void OpenCamera()
        {
            using (Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture))
            {
                StartActivityForResult(intent, TAKE_PICTURE_CODE);
            }
        }

        /// <summary>
        /// Метод обработки изображения и смена лэйаута приложения
        /// </summary>
        /// <param name="intent">Intent.</param>
        private void ProcessCameraImage(Intent intent)
        {
            //Меняем основное окно на окно захвата изображения
            SetContentView(Resource.Layout.detectlayout);
            //Повесим событие на кнопку определения лиц
            ((Button)FindViewById(Resource.Id.detect_face)).Click += btnDetect_HandleClick;
            ((Button)FindViewById(Resource.Id.back)).Click += btnBack_HandleClick;

            //Получаем изображения из элемента ImageView
            ImageView imageView = (ImageView)FindViewById(Resource.Id.image_view);
            try
            {
                cameraBitmap = (Bitmap)intent.Extras.Get("data");
                //Вставляем изображение из CameraBitmap
                imageView.SetImageBitmap(cameraBitmap);
            }
            catch(Exception e)
            {
                Toast.MakeText(this, "Фото не сделанно!", ToastLength.Short).Show();
                //cameraBitmap.Dispose();
            }
           
        }

      
        /// <summary>
        /// Детектирование лиц и прорисовка квадрата на каждом из лиц.
        /// </summary<
        private void detectFaces()
        {
            // Проверка на получения картинки 
            if (null != cameraBitmap)
            {
                //Получаем ширину
                int width = cameraBitmap.Width;
                //Получаем высоту
                int height = cameraBitmap.Height;
                //Создаем экземпляр класса нативных библиотек распознования от Android
                FaceDetector detector = new FaceDetector(width, height, MAX_FACES);
                //Создаем массив лиц
                Android.Media.FaceDetector.Face[] faces = new Android.Media.FaceDetector.Face[MAX_FACES];

                //Создаем основной Bitmap
                Bitmap bitmap565 = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
                Paint ditherPaint = new Paint();
                Paint drawPaint = new Paint();

                ditherPaint.Dither = true;
                //Устанавливаем цвет квадрата, штриховку, толщину 
                drawPaint.Color = Color.Red;
                drawPaint.SetStyle(Paint.Style.Stroke);
                drawPaint.StrokeWidth = 2;

                //Создаем холст и устанавливаем
                Canvas canvas = new Canvas();
                canvas.SetBitmap(bitmap565);
                canvas.DrawBitmap(cameraBitmap, 0, 0, ditherPaint);

                //Получаем количество лиц
                int facesFound = detector.FindFaces(bitmap565, faces);
                //Средняя точка
                PointF midPoint = new PointF();
                //Расстояние до глаз
                float eyeDistance = 0.0f;
                float confidence = 0.0f;
                //Печать в консоль для тестирования приложения
                System.Console.WriteLine("Найдено лиц: " + facesFound);

                //Проверка, что найдено хоть одно лицо
                if (facesFound > 0)
                {
                    //Обводим каждое лицо красным квадратом
                    for (int index = 0; index < facesFound; ++index)
                    {
                        
                        faces[index].GetMidPoint(midPoint);
                        eyeDistance = faces[index].EyesDistance();
                        confidence = faces[index].Confidence();
                        //Печатаем для отладки в консоль
                        System.Console.WriteLine("Коэфициент доверия: " + confidence +
                         ", Расстояние до глаз: " + eyeDistance +
                         ", Средняя точка: (" + midPoint.X + ", " + midPoint.Y + ")");

                        //Передаем в TextView значение расстояния до лица
                        ((TextView)FindViewById(Resource.Id.test)).Text = eyeDistance.ToString() + " см";
                        ((TextView)FindViewById(Resource.Id.test)).SetTextColor(Color.Aqua);
                        ((TextView)FindViewById(Resource.Id.test)).SetTextSize(Android.Util.ComplexUnitType.Sp, 26);

                        //Рисуем квадрат
                        canvas.DrawRect((int)midPoint.X - eyeDistance,
                         (int)midPoint.Y - eyeDistance,
                         (int)midPoint.X + eyeDistance,
                         (int)midPoint.Y + eyeDistance, drawPaint);
                    }
                }

                ImageView imageView = (ImageView)FindViewById(Resource.Id.image_view);
                imageView.SetImageBitmap(bitmap565);
                
            }
        }
    }

}


