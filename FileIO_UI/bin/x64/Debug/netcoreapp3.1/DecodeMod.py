import cv2 
import os
import sys
import getopt

# save_dir = ""

try:
    opts, args = getopt.getopt(sys.argv[1:],"help i:o:",["infile=","outfile="])
except getopt.GetoptError:
    print("help: -i  inputfile  -o  outputpath")
    sys.exit()
for opt, arg in opts:
    if opt == '-h':
        print("help: -i  inputfile  -o  outputpath")
        sys.exit()
    elif opt =='-i':
        video = arg
    elif opt == '-o':
        save_dir = arg

# video = r"Z:/Common/20191130数据备份/cam678/2019-11-29-1-6-25-471(slow)/EncodeResult/Camera00D36305969_Image/Camera_00D36305969.mjpeg"
# save_dir = r"D:/frames/cam678/2019-11-29-1-6-25-471(slow)/EncodeResult/Camera00D36305969_Image/Camera_00D36305969/"

if not os.path.exists(save_dir):
    os.makedirs(save_dir)

#获取文件大小
sum_size = os.path.getsize(video)
single_size = sum_size
print(sum_size)

cap = cv2.VideoCapture(video)
# frame_sum = cap.get(7)
# print(frame_sum)

count = 1
is_first = True
while True:
    has_frame, frame = cap.read()
    if has_frame:
        filepath = save_dir + '/f' + str(count) + '.jpg'
        cv2.imwrite(filepath, frame)
        #计算解压进度 
        if is_first:
            single_size = os.path.getsize(filepath)
            is_first = False
        elif count%15==0:
            single_size = (single_size*(count-1)+os.path.getsize(filepath))/count
            # print(str(count*single_size*0.813*100/sum_size) + "%",end='\r')
            print("---->{:.1f}%".format(count*single_size*100/sum_size), end='\r')
        count += 1
    else:
        print('---->Finished!')
        break
#sys.exit()
os._exit(0)