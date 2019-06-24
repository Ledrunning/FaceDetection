using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Widget;
using static Test.Config.ProgramConstants;

//TODO: Захват растояния в реальном времени с камеры, отрисовка прямоугольника 
//при фиксировании лица
//Сохранение фото
//Применение фрагментов, вместо переключения Activity

namespace Test
{
    [Activity(Label = "Распознаватель v1.1B", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        //Получил Bitmap из картинки
        private Bitmap cameraBitmap;

        private ImageView imageView;
        //Intent intent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Установка основного экрана
            SetContentView(Resource.Layout.Main);
            //Событие на кнопку для кнопки сделать фото;
            ((Button) FindViewById(Resource.Id.btnTake_picture)).Click += btntake_HandleClick;
            imageView = (ImageView) FindViewById(Resource.Id.image_view);
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            //Проверяем запрос кода с константой
            if (TAKE_PICTURE_CODE == requestCode) ProcessCameraImage(data);
        }

        /// <summary>
        ///     Метод для открытия камеры
        /// </summary>
        private void OpenCamera()
        {
            using (var intent = new Intent(MediaStore.ActionImageCapture))
            {
                StartActivityForResult(intent, TAKE_PICTURE_CODE);
            }
        }

        /// <summary>
        ///     Метод обработки изображения и смена лэйаута приложения
        /// </summary>
        /// <param name="intent">Intent.</param>
        private void ProcessCameraImage(Intent intent)
        {
            //Меняем основное окно на окно захвата изображения
            SetContentView(Resource.Layout.detectlayout);
            //Повесим событие на кнопку определения лиц
            ((Button) FindViewById(Resource.Id.btnDetect_face)).Click += btnDetect_HandleClick;
            ((Button) FindViewById(Resource.Id.btnBack)).Click += btnBack_HandleClick;

            //Получаем изображения из элемента ImageView
            //ImageView imageView = (ImageView)FindViewById(Resource.Id.image_view);
            try
            {
                cameraBitmap = (Bitmap) intent.Extras.Get("data");
                //Вставляем изображение из CameraBitmap
                imageView.SetImageBitmap(cameraBitmap);
            }
            catch (Exception e)
            {
                Toast.MakeText(this, "Фото не сделанно!", ToastLength.Short).Show();
                //cameraBitmap.Dispose();
            }
        }


        /// <summary>
        ///     Детектирование лиц и прорисовка квадрата на каждом из лиц.
        /// </summary
        /// <
        private void detectFaces()
        {
            // Проверка на получения картинки 
            if (null != cameraBitmap)
            {
                //Получаем ширину
                var width = cameraBitmap.Width;
                //Получаем высоту
                var height = cameraBitmap.Height;
                //Создаем экземпляр класса нативных библиотек распознования от Android
                var detector = new FaceDetector(width, height, MAX_FACES);
                //Создаем массив лиц
                var faces = new FaceDetector.Face[MAX_FACES];
                //Создаем основной Bitmap
                var bitmap565 = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
                var ditherPaint = new Paint();
                //Рамка захвата
                var drawPaint = new Paint();
                ditherPaint.Dither = true;
                //Устанавливаем цвет квадрата, штриховку, толщину 
                drawPaint.Color = Color.Blue;
                drawPaint.SetStyle(Paint.Style.Stroke);
                drawPaint.StrokeWidth = 2;
                //Создаем холст и устанавливаем
                var canvas = new Canvas();
                canvas.SetBitmap(bitmap565);
                canvas.DrawBitmap(cameraBitmap, 0, 0, ditherPaint);
                //Получаем количество лиц
                var facesFound = detector.FindFaces(bitmap565, faces);
                //Средняя точка
                var midPoint = new PointF();
                //Расстояние до глаз
                var eyeDistance = 0.0f;
                var confidence = 0.0f;
                //Печать в консоль для тестирования приложения
                Console.WriteLine("Найдено лиц: " + facesFound);
                //Проверка, что найдено хоть одно лицо
                if (facesFound > 0)
                    for (var index = 0; index < facesFound; ++index)
                    {
                        faces[index].GetMidPoint(midPoint);
                        eyeDistance = faces[index].EyesDistance();
                        confidence = faces[index].Confidence();
                        //Печатаем для отладки в консоль
                        Console.WriteLine("Коэфициент доверия: " + confidence +
                                          ", Расстояние до глаз: " + eyeDistance +
                                          ", Средняя точка: (" + midPoint.X + ", " + midPoint.Y + ")");


                        //Передаем в TextView значение расстояния до лица
                        ((TextView) FindViewById(Resource.Id.tvDistanceToCamera)).Text =
                            string.Format("{0:0.00} см", eyeDistance);
                        //((TextView)FindViewById(Resource.Id.tvDistanceToCamera)).SetTextColor(Color.Aqua);
                        //((TextView)FindViewById(Resource.Id.tvDistanceToCamera)).SetTextSize(Android.Util.ComplexUnitType.Sp, 26);

                        //Рисуем квадрат
                        canvas.DrawRect((int) midPoint.X - eyeDistance,
                            (int) midPoint.Y - eyeDistance,
                            (int) midPoint.X + eyeDistance,
                            (int) midPoint.Y + eyeDistance, drawPaint);
                    }

                var imageView = (ImageView) FindViewById(Resource.Id.image_view);
                imageView.SetImageBitmap(bitmap565);
            }
        }

        #region Button Handlers

        //Обработчик для открытия камеры
        private void btntake_HandleClick(object sender, EventArgs e)
        {
            //Вызов события OpenCamera() 
            OpenCamera();
        }

        //Обработчик кнопки определения лиц
        private void btnDetect_HandleClick(object sender, EventArgs e)
        {
            //Вызываем метод определения лиц
            detectFaces();
        }

        //Обработчик кнопки назад
        private void btnBack_HandleClick(object sender, EventArgs e)
        {
            FinishActivity(Resource.Layout.detectlayout);
            SetContentView(Resource.Layout.Main);
            //Событие на кнопку для кнопки сделать фото;
            ((Button) FindViewById(Resource.Id.btnTake_picture)).Click += btntake_HandleClick;
        }

        #endregion
    }
}