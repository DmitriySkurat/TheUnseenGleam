"""
Генератор файла Игровой цикл.vsdx для проекта The Unseen Gleam.
Основан на структуре оригинального файла-примера (Quest-игра).

Точки соединения (Connection points) для каждого типа мастера:
  Circle (Master=73):   X1=bottom, X2=right,  X3=top,    X4=left
  Rect   (Master=74):   X1=left,   X2=right,  X3=bottom, X4=top
  Diamond(Master=58):   X1=left,   X2=right,  X3=top,    X4=bottom
"""

import zipfile, shutil, os, textwrap
from xml.etree import ElementTree as ET

SRC  = r"D:\Temp\game_loop_copy.vsdx"
DEST = r"D:\BSUIR\Diploma\TheUnseenGleam\Игровой цикл.vsdx"

NSMAP = {
    "main": "http://schemas.microsoft.com/office/visio/2012/main",
    "r":    "http://schemas.openxmlformats.org/officeDocument/2006/relationships",
}
NS = NSMAP["main"]

# ─── helpers ──────────────────────────────────────────────────────────────────

def conn(shape, point):
    """Return (abs_x, abs_y) for a connection point on a shape dict."""
    cx, cy, w, h = shape["x"], shape["y"], shape["w"], shape["h"]
    master = shape["master"]
    # Circle (73)
    if master == 73:
        pts = {1: (cx, cy - h/2), 2: (cx + w/2, cy),
               3: (cx, cy + h/2), 4: (cx - w/2, cy)}
    # Rect (74)
    elif master == 74:
        pts = {1: (cx - w/2, cy), 2: (cx + w/2, cy),
               3: (cx, cy - h/2), 4: (cx, cy + h/2)}
    # Diamond (58)
    elif master == 58:
        pts = {1: (cx - w/2, cy), 2: (cx + w/2, cy),
               3: (cx, cy + h/2), 4: (cx, cy - h/2)}
    else:
        pts = {1: (cx, cy - h/2), 2: (cx + w/2, cy),
               3: (cx, cy + h/2), 4: (cx - w/2, cy)}
    return pts[point]


def shape_xml(sid, master, text, x, y, w, h):
    """Minimal shape XML (no inherited style overrides — uses master theme)."""
    lx = w / 2
    ly = h / 2
    return f"""<Shape ID="{sid}" NameU="Shape" Name="Фигура" Type="Shape" Master="{master}" xmlns="{NS}">
  <Cell N="PinX" V="{x:.6f}"/>
  <Cell N="PinY" V="{y:.6f}"/>
  <Cell N="Width" V="{w:.6f}"/>
  <Cell N="Height" V="{h:.6f}"/>
  <Cell N="LocPinX" V="{lx:.6f}" F="Width*0.5"/>
  <Cell N="LocPinY" V="{ly:.6f}" F="Height*0.5"/>
  <Cell N="ObjType" V="1"/>
  <Text>{text}</Text>
</Shape>"""


def connector_xml(sid, from_shape, from_pt, to_shape, to_pt, label=""):
    """Dynamic connector between two shapes."""
    bx, by = conn(from_shape, from_pt)
    ex, ey = conn(to_shape, to_pt)
    px = (bx + ex) / 2
    py = (by + ey) / 2
    cw = ex - bx
    ch = ey - by
    lpx = cw / 2 if cw != 0 else 0
    lpy = ch / 2 if ch != 0 else 0
    fid = from_shape["id"]
    tid = to_shape["id"]
    fpn = from_pt
    tpn = to_pt
    text_tag = f"  <Text>{label}</Text>\n" if label else ""
    return f"""<Shape ID="{sid}" NameU="Dynamic connector" Type="Shape" Master="64" xmlns="{NS}">
  <Cell N="PinX" V="{px:.6f}" F="Inh"/>
  <Cell N="PinY" V="{py:.6f}" F="Inh"/>
  <Cell N="Width" V="{cw:.6f}" F="GUARD(EndX-BeginX)"/>
  <Cell N="Height" V="{ch:.6f}" F="GUARD(EndY-BeginY)"/>
  <Cell N="LocPinX" V="{lpx:.6f}" F="Inh"/>
  <Cell N="LocPinY" V="{lpy:.6f}" F="Inh"/>
  <Cell N="BeginX" V="{bx:.6f}" F="PAR(PNT(Sheet.{fid}!Connections.X{fpn},Sheet.{fid}!Connections.Y{fpn}))"/>
  <Cell N="BeginY" V="{by:.6f}" F="PAR(PNT(Sheet.{fid}!Connections.X{fpn},Sheet.{fid}!Connections.Y{fpn}))"/>
  <Cell N="EndX"   V="{ex:.6f}" F="PAR(PNT(Sheet.{tid}!Connections.X{tpn},Sheet.{tid}!Connections.Y{tpn}))"/>
  <Cell N="EndY"   V="{ey:.6f}" F="PAR(PNT(Sheet.{tid}!Connections.X{tpn},Sheet.{tid}!Connections.Y{tpn}))"/>
  <Cell N="BegTrigger" V="2" F="_XFTRIGGER(Sheet.{fid}!EventXFMod)"/>
  <Cell N="EndTrigger" V="2" F="_XFTRIGGER(Sheet.{tid}!EventXFMod)"/>
{text_tag}</Shape>"""


# ─── shapes definition ────────────────────────────────────────────────────────
# (id, master, text, x, y, w, h)
# Master 73 = Circle/Oval,  74 = Rectangle process,  58 = Diamond decision

CW = 3.8   # circle width
CH = 1.4   # circle height
RW = 5.5   # rect width
RH = 1.4   # rect height
DW = 6.0   # diamond width
DH = 2.8   # diamond height

SHAPES_DEF = [
    #  id    M   text                                              x      y     w    h
    (2000,  73, "Старт",                                         10.5, 29.8,  CW,  CH),
    (2001,  74, "Загрузка игрового приложения",                  10.5, 28.0,  RW,  RH),
    (2002,  73, "Главное меню",                                  10.5, 26.3,  CW,  CH),
    (2003,  73, "Настройки",                                     17.5, 26.3,  CW,  CH),
    (2004,  73, "Выход из игры",                                  3.5, 26.3,  CW,  CH),
    (2005,  74, "Загрузка игровой сессии и уровня",              10.5, 24.5,  RW,  RH),
    (2006,  73, "Перемещение по уровню",                          5.5, 22.8,  CW,  CH),
    (2007,  73, "Взаимодействие с объектами\nи предметами",      15.5, 22.8, 4.5,  CH),
    (2008,  73, "Управление шумом\nи освещённостью",             10.5, 21.0, 4.8,  CH),
    (2009,  58, "Охранник обнаружил игрока?",                    10.5, 18.5,  DW,  DH),
    (2010,  73, "Охранник патрулирует маршрут",                   3.5, 18.5,  CW,  CH),
    (2011,  74, "Охранники преследуют игрока",                   10.5, 15.8, 5.0,  RH),
    (2012,  58, "Игрок нейтрализовал охранника?",                10.5, 12.5,  DW,  DH),
    (2013,  73, "Гибель игрока /\nзагрузка с чекпоинта",         17.5, 12.5,  CW,  CH),
    (2014,  58, "Игрок достиг выхода\nиз уровня?",               10.5,  9.5,  DW,  DH),
    (2015,  74, "Сохранение прогресса",                          10.5,  7.2,  RW,  RH),
    (2016,  58, "Пройдены все уровни?",                           7.5,  4.5,  DW,  DH),
    (2017,  73, "Конец",                                         15.5,  4.5,  CW,  CH),
]

# Build lookup: id -> shape dict
SHAPES = {sid: {"id": sid, "master": m, "text": t, "x": x, "y": y, "w": w, "h": h}
          for (sid, m, t, x, y, w, h) in SHAPES_DEF}


# ─── connectors definition ────────────────────────────────────────────────────
# (conn_id, from_id, from_pt, to_id, to_pt, label)
#   Connection points:
#     Circle  73: 1=bottom, 2=right, 3=top, 4=left
#     Rect    74: 1=left,   2=right, 3=bottom, 4=top
#     Diamond 58: 1=left,   2=right, 3=top,    4=bottom

CONNS_DEF = [
    # Start → Load App → Menu
    (3000, 2000, 1, 2001, 4, ""),          # Старт bottom → Загрузка приложения top
    (3001, 2001, 3, 2002, 3, ""),          # Загрузка приложения bottom → Главное меню top
    # Menu branches
    (3002, 2002, 2, 2003, 4, ""),          # Меню right → Настройки left
    (3003, 2002, 4, 2004, 2, ""),          # Меню left → Выход из игры right
    (3004, 2002, 1, 2005, 4, ""),          # Меню bottom → Загрузка уровня top
    # Session/level load → gameplay actions
    (3005, 2005, 1, 2006, 2, ""),          # Загрузка уровня left → Перемещение right
    (3006, 2005, 2, 2007, 4, ""),          # Загрузка уровня right → Взаимодействие left
    (3007, 2006, 1, 2008, 4, ""),          # Перемещение bottom → Управление шумом left
    (3008, 2007, 1, 2008, 2, ""),          # Взаимодействие bottom → Управление шумом right
    # Шум/свет → охранник обнаружил?
    (3009, 2008, 1, 2009, 3, ""),          # Управление шумом bottom → Охранник? top
    # Охранник не обнаружил
    (3010, 2009, 1, 2010, 2, "Нет"),       # Охранник? left (Нет) → Патрулирует right
    (3011, 2010, 3, 2006, 3, ""),          # Патрулирует top → Перемещение top (loop back)
    # Охранник обнаружил
    (3012, 2009, 4, 2011, 4, "Да"),        # Охранник? bottom (Да) → Преследует top
    # Преследуют → нейтрализовал?
    (3013, 2011, 3, 2012, 3, ""),          # Преследует bottom → Нейтрализовал? top
    # Нейтрализовал? ветки
    (3014, 2012, 2, 2013, 4, "Нет"),       # Нейтрализовал? right (Нет) → Гибель left
    (3015, 2013, 3, 2005, 2, ""),          # Гибель top → Загрузка уровня right (loop)
    (3016, 2012, 4, 2014, 3, "Да"),        # Нейтрализовал? bottom (Да) → Достиг выхода? top
    # Достиг выхода?
    (3017, 2014, 1, 2008, 4, "Нет"),       # Достиг выхода? left (Нет) → Управление шумом left (loop)
    (3018, 2014, 4, 2015, 4, "Да"),        # Достиг выхода? bottom (Да) → Сохранение top
    # Сохранение → все уровни?
    (3019, 2015, 3, 2016, 3, ""),          # Сохранение bottom → Все уровни? top
    # Все уровни?
    (3020, 2016, 1, 2005, 1, "Нет"),       # Все уровни? left (Нет) → Загрузка уровня left (loop)
    (3021, 2016, 2, 2017, 4, "Да"),        # Все уровни? right (Да) → Конец left
]

# ─── stamp text to update ─────────────────────────────────────────────────────
TITLE_TEXT = ("Компьютерная игра в жанре\nстелс-платформер.\n"
              "Игровой цикл.")

# ─── read original page XML ───────────────────────────────────────────────────
ET.register_namespace("", NS)
ET.register_namespace("r", NSMAP["r"])

with zipfile.ZipFile(SRC, "r") as z:
    page_bytes = z.read("visio/pages/page1.xml")

page_xml_str = page_bytes.decode("utf-8")
root = ET.fromstring(page_xml_str)

# Prefix helper
def tag(name):
    return f"{{{NS}}}{name}"

shapes_elem = root.find(tag("Shapes"))

# ─── remove old game-logic shapes (IDs 994–1028) ─────────────────────────────
OLD_IDS = set(range(994, 1029))
to_remove = [s for s in shapes_elem if s.get("ID") and int(s.get("ID", 0)) in OLD_IDS]
for s in to_remove:
    shapes_elem.remove(s)

# ─── update title text (shape 1032) ──────────────────────────────────────────
for s in shapes_elem:
    if s.get("ID") == "1032":
        txt = s.find(tag("Text"))
        if txt is not None:
            txt.text = TITLE_TEXT

# ─── insert new shapes ────────────────────────────────────────────────────────
# Parse and append each new shape
for (sid, m, text, x, y, w, h) in SHAPES_DEF:
    xml_str = shape_xml(sid, m, text, x, y, w, h)
    elem = ET.fromstring(xml_str)
    shapes_elem.append(elem)

# ─── insert connectors ────────────────────────────────────────────────────────
for (cid, fid, fpt, tid, tpt, lbl) in CONNS_DEF:
    xml_str = connector_xml(cid, SHAPES[fid], fpt, SHAPES[tid], tpt, lbl)
    elem = ET.fromstring(xml_str)
    shapes_elem.append(elem)

# ─── serialise modified XML ───────────────────────────────────────────────────
new_page_bytes = ET.tostring(root, encoding="utf-8", xml_declaration=True)

# ─── write new vsdx ──────────────────────────────────────────────────────────
TMP = DEST + ".tmp"
with zipfile.ZipFile(SRC, "r") as zin, zipfile.ZipFile(TMP, "w", zipfile.ZIP_DEFLATED) as zout:
    for item in zin.infolist():
        if item.filename == "visio/pages/page1.xml":
            zout.writestr(item, new_page_bytes)
        else:
            zout.writestr(item, zin.read(item.filename))

if os.path.exists(DEST):
    os.remove(DEST)
os.rename(TMP, DEST)
print(f"Done: {DEST}")
