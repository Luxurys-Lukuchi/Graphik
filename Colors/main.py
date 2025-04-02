from PIL import Image
import matplotlib.pyplot as plt
import numpy as np
from tkinter import Tk, filedialog


def analyze_image(image_path):
    try:
        img = Image.open(image_path).convert('RGB')
    except Exception as e:
        print(f"Ошибка: {e}")
        return

    # Преобразуем изображение в NumPy массив
    img_array = np.array(img)
    height, width, _ = img_array.shape
    total_pixels = width * height

    # Считаем суммы каналов
    total_red = np.sum(img_array[:, :, 0])
    total_green = np.sum(img_array[:, :, 1])
    total_blue = np.sum(img_array[:, :, 2])

    # Построение диаграммы
    plt.figure(figsize=(10, 6))
    colors = ['red', 'green', 'blue']
    values = [total_red, total_green, total_blue]
    plt.bar(colors, values, color=colors)
    plt.title('Суммарные значения цветовых каналов')
    plt.xlabel('Цвет')
    plt.ylabel('Сумма значений')
    plt.show()

    # Вывод результатов
    print("\nРезультаты анализа:")
    print(f"Размер изображения: {width}x{height}")
    print(f"Общее количество пикселей: {total_pixels}")
    print(f"Сумма красного: {total_red}")
    print(f"Сумма зеленого: {total_green}")
    print(f"Сумма синего: {total_blue}")


if __name__ == "__main__":
    root = Tk()
    root.withdraw()
    file_path = filedialog.askopenfilename(
        title="Выберите изображение",
        filetypes=[("Изображения", "*.jpg *.jpeg *.png *.bmp *.gif")]
    )
    root.destroy()

    if file_path:
        analyze_image(file_path)
    else:
        print("Файл не выбран!")