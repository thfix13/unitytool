


import numpy 
import pylab 
from scipy.stats import norm
from numpy import linspace
from pylab import plot,show,hist,figure,title
import matplotlib.pyplot as plt
import numpy as np




dataTriangle = [212, 164, 40, 164, 116, 168, 76, 168, 298, 164, 172, 164, 168, 164, 100, 40, 212, 164, 164, 134, 156, 80, 274, 164, 126, 56, 164, 164, 198, 40, 80]
dataGerman = [1102, 1484, 1098, 1232, 1136, 1184, 1164, 1264, 1502, 1906, 2116, 1826, 1456, 1310, 1876, 966, 1382, 2024, 1432, 1076, 1220, 878, 1184, 1930, 1408, 1064, 2028, 1418, 1094, 1296, 986]
dataFrench = [1052, 1704, 2078, 1890, 878, 1628, 856, 1814, 828, 1738, 1600, 2066, 838, 1772, 1550, 846, 936, 830, 1784, 1768, 838, 1870, 1010, 1892, 1656, 1814, 1814, 874, 1562, 890, 986]


xmaxData = max(dataFrench);
ymaxData = 13

colls = dataTriangle;

mu, std = norm.fit(colls)
med = numpy.median(colls)

a = 0.6
b = 50




fig =plt.figure(1)
fig.subplots_adjust(hspace=0.05)
plt.subplot(311)
plt.subplot(311).set_xticklabels([])

plt.hist(colls,bins=b,range=(0,xmaxData), normed=0,color = '#0000ff',alpha=a)
# plt.title("avg = " + str(mu) + ", std =" + str(std) + ", med =" + str(med))
plt.axis([0,xmaxData,0,ymaxData])

plt.subplot(311)
xmin, xmax = plt.xlim()
x = np.linspace(xmin, xmax, 600)
p = norm.pdf(x, mu, std) 
p*=100
p*=ymaxData
plt.plot(x, p, 'k', linewidth=1.4,color='#0000ff')

#med and avg
plt.axvline(mu, color='#0000ff', linestyle='dashed', linewidth=1.2)
plt.axvline(med, color='#0000ff', linestyle='-.', linewidth=1.2)

#PLOT 2
colls = dataFrench;

mu, std = norm.fit(colls)
med = numpy.median(colls)

plt.subplot(312)
plt.subplot(312).set_xticklabels([])
plt.hist(colls,bins=b,range=(0,xmaxData), normed=0,color = '#ff00ff',alpha=a)
# plt.title("avg = " + str(mu) + ", std =" + str(std) + ", med =" + str(med))


plt.axis([0,xmaxData,0,ymaxData])


xmin, xmax = plt.xlim()
x = np.linspace(xmin, xmax, 600)
p = norm.pdf(x, mu, std)
p*=100
p*=ymaxData
plt.plot(x, p, 'k', linewidth=1.4,color='#ff00ff')

#med and avg
plt.axvline(mu, color='#ff00ff', linestyle='dashed', linewidth=1.2)
plt.axvline(med, color='#ff00ff', linestyle='-.', linewidth=1.2)

#PLOT 3
colls = dataGerman;

mu, std = norm.fit(colls)
med = numpy.median(colls)

plt.subplot(313)
plt.hist(colls,bins=b,range=(0,xmaxData), normed=0,color = '#0EB554',alpha=a)
# plt.title("avg = " + str(mu) + ", std =" + str(std) + ", med =" + str(med))

xmin, xmax = plt.xlim()
x = np.linspace(xmin, xmax, 600)
p = norm.pdf(x, mu, std)
p*=100
p*=ymaxData
plt.plot(x, p, 'k', linewidth=1.4,color='#0EB554')

plt.axis([0,xmaxData,0,ymaxData])

plt.axvline(mu, color='#0EB554', linestyle='dashed', linewidth=1.2)
plt.axvline(med, color='#0EB554', linestyle='-.', linewidth=1.2)

plt.savefig("Data_results.pdf",bbox_inches='tight')

plt.show()