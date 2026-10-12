from PIL import Image, ImageDraw
from pathlib import Path
import math, random, wave, struct

root = Path(__file__).resolve().parents[1]
art = root / 'Assets/Resources/Art'
audio = root / 'Assets/Resources/Audio'
random.seed(19)

def sprite(name, paint):
    im = Image.new('RGBA', (32, 32))
    paint(ImageDraw.Draw(im))
    im.save(art / (name + '.png'))

def ship(d):
    d.polygon([(16,1),(20,12),(27,23),(27,27),(19,23),(16,27),(13,23),(5,27),(5,23),(12,12)], fill='#7299aa', outline='#263f50')
    d.polygon([(16,5),(19,16),(16,20),(13,16)], fill='#98dddd')
    d.rectangle((14,23,18,28),fill='#454b61')
    d.rectangle((15,28,17,31),fill='#ffb553')
sprite('player', ship)

def enemy(d, kind):
    if kind == 0:
        d.polygon([(6,5),(13,9),(19,9),(26,5),(29,20),(23,26),(9,26),(3,20)],fill='#b95d68',outline='#603943')
        d.rectangle((12,12,20,17),fill='#f1ba80')
        d.rectangle((6,19,9,28),fill='#775075'); d.rectangle((23,19,26,28),fill='#775075')
    else:
        d.polygon([(16,3),(29,14),(24,26),(8,26),(3,14)],fill='#9c82ad',outline='#463853')
        d.rectangle((11,10,21,16),fill='#c8dfa0')
        d.rectangle((13,20,19,29),fill='#594b72')
for k in range(2): sprite('enemy'+str(k),lambda d,k=k: enemy(d,k))

def rock(d, damage):
    d.polygon([(10,2),(22,3),(29,11),(27,23),(20,30),(8,27),(2,17),(4,8)],fill=['#93919a','#a39c94','#b8a292'][damage],outline='#514f5b')
    d.ellipse((8,7,15,13),fill='#686872'); d.ellipse((18,18,23,24),fill='#686872')
    if damage: d.line([(17,3),(14,11),(19,16),(12,23),(14,28)],fill='#443d4d',width=2)
    if damage == 2: d.line([(3,16),(13,15),(19,20),(27,21)],fill='#443d4d',width=2)
for k in range(3): sprite('rock'+str(k),lambda d,k=k: rock(d,k))

sprite('bullet0',lambda d: d.rectangle((13,3,18,28),fill='#f5d577',outline='#9f713e'))
sprite('bullet1',lambda d: d.polygon([(16,1),(24,15),(16,30),(8,15)],fill='#8ce3ca',outline='#318578'))
sprite('enemybullet0',lambda d: d.ellipse((7,5,24,26),fill='#e68675',outline='#9b4351',width=2))
sprite('enemybullet1',lambda d: d.polygon([(16,2),(27,16),(16,29),(5,16)],fill='#bf9bde',outline='#72588c',width=2))

def heart(d):
    d.polygon([(3,7),(7,3),(12,3),(16,8),(20,3),(25,3),(29,7),(29,14),(16,29),(3,14)],fill='#d77a80',outline='#743c58')
sprite('life',heart)

def power(d, k):
    d.rectangle((2,2,29,29),fill=['#526880','#4d8075','#835667'][k],outline='#c3c7c9',width=2)
    if k == 0:
        for x in [8,15,22]: d.line((x,21,x,10),fill='#f4d685',width=2); d.polygon([(x-3,12),(x,7),(x+3,12)],fill='#f4d685')
    elif k == 1: d.polygon([(17,5),(9,18),(15,18),(13,27),(23,13),(17,13)],fill='#b2edd5')
    else: d.rectangle((13,7,18,24),fill='#f1c1c4'); d.rectangle((7,13,24,18),fill='#f1c1c4')
for k in range(3): sprite('power'+str(k),lambda d,k=k: power(d,k))

im = Image.new('RGB',(1280,720),'#11172a')
d=ImageDraw.Draw(im)
for y in range(720):
    c=int(5*y/720); d.line((0,y,1279,y),fill=(17+c,23+c,42+c))
for _ in range(180):
    x=random.randrange(1280); y=random.randrange(720); shade=random.choice(['#556478','#81909f','#a2a9b3'])
    d.rectangle((x,y,x+random.choice([1,1,2]),y+1),fill=shade)
d.ellipse((1040,490,1180,630),fill='#27304a',outline='#39415a',width=3)
d.ellipse((1062,517,1090,542),fill='#30394f'); d.ellipse((1112,563,1150,596),fill='#30394f')
im.save(art/'background.png')

def tone(name, duration, frequency, end=None, noise=0):
    rate=22050
    data=[]
    for i in range(int(rate*duration)):
        t=i/rate; f=frequency if end is None else frequency+(end-frequency)*t/duration
        signal=(math.sin(2*math.pi*f*t)*.65 + random.uniform(-1,1)*noise)
        envelope=(1-t/duration)**1.5 * min(1,t/.008)
        data.append(int(max(-1,min(1,signal*envelope)) * 16000))
    with wave.open(str(audio/(name+'.wav')),'wb') as w:
        w.setparams((1,2,rate,0,'NONE','not compressed')); w.writeframes(struct.pack('<'+'h'*len(data),*data))
for name,dur,start,end,noise in [('shot',.1,1000,400,0),('special',.16,550,1500,0),('hit',.25,180,70,.3),('pickup',.28,450,1100,0),('rock',.12,130,60,.45),('metal',.14,650,180,.15),('cancel',.09,1700,400,.1),('explosion',.35,110,25,.6),('win',.65,400,1000,0),('lose',.6,350,80,0)]:
    tone(name,dur,start,end,noise)
import runpy
runpy.run_path(str(root/'Validation/create_music.py'))

preview=Image.new('RGB',(640,280),'#11172a')
for i,p in enumerate(sorted(art.glob('*.png'))):
    if p.stem=='background': continue
    icon=Image.open(p).resize((64,64),Image.Resampling.NEAREST)
    x=(i%8)*80; y=(i//8)*110
    preview.paste(icon,(x+8,y+4),icon); ImageDraw.Draw(preview).text((x+2,y+75),p.stem,fill='white')
preview.save(root/'Validation/sprites.png')
