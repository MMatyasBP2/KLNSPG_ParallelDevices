# Párhuzamos eszközök programozása - féléves feladat leírása
Alapvető matematikai statisztikák, mint az Összeg, Átlag, Minimum, Maximum és Medián értékek számítása CPU-n és GPU-n C# host programmal.
A beadandó bizonyítja, hogy a statisztikák számítása, megfelelő workitem szám használatával GPU-n sokkal gyorsabb eredményt ad, mint a CPU-n számított statisztikák.

A szoftver, első körben képes a ,,calculations.cl'' OpenCL kernel fájl felolvasására, majd a Cloo NuGet package segítségével egy olyan host programot reprezentál, mely bármelyik kernel függvény felolvasására és lefuttatására funkcionál.

A szoftver futtatása után a felhasználó beállíthatja a probléma méretét, majd a ,,START MEASUREMENT'' gombra kattintva, elindíthatja a mérési folyamatot.
A mérés során a program párhuzamosan dolgozza fel az egyes számításokat, és egy progress bar jelzi az éppen aktuálisan feldolgozott számítást, így a felhasználó mindig tudja, hogy hol tart éppen a program kalkulációja.
A futás végén, a szoftver, szövegdobozokba írja az eredményeket, és kimenti őket egy CSV fájlba is, amelyben további diagrammok készülhetnek a futási eredményekről.

![attachment1](attachment1.png)
*A szoftver, futás közben*

![attachment2](attachment2.png)
*A számítások eredményei*

Az alábbi mérésről létrehozott diagrammok szemléletesebben kiemelik a CPU és GPU futási idők közti különbségeket:

![attachment3](attachment3.png)
*Mérési különbségek*

Az Excel fájl kördiagrammjai jól mutatják, hogy CPU és GPU futtatás eltérő időeredményeket produkál. A legkiemelkedőbb tán a medián értékszámítás, ahol ugyan az az algoritmus fut CPU-n, mint GPU-n, a legtöbb workitemet-et kihasználva, így kimagaslóan jobb időeredménnyel fut le, mintha a processzor magjai számolnák a minta mediánját.
