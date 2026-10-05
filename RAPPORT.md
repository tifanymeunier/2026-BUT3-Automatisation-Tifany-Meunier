Tifany
Meunier
33a

TP1

nom de l'image créée : tp-api

tag de l’image : tp1

# Pourquoi proxy_pass peut-il désigner l'hôte api alors que ce nom n'existe nulle part sur votre machine ? : 

car api est l'hote par défaut 

# À quoi sert la ligne try_files $uri $uri/ /index.html ? Que se passerait-il sans elle si l'utilisateur rechargeait la page sur /tasks ? :

 Elle sert à tester les chemins disponibles, si on recharge la page sur /tasks cela va provoquer une erreur



# taille des images :

tp-automatisation-api:latest  2.01GB 

tp-automatisation-front:latest  63.4MB


## TP2

backend : 

reconstruction : real    0m0,332s

après modif : real    0m0,389s

user : uid=0(root) gid=0(root) groups=0(root)


frontend : 

reconstruction : real    0m30,958s

après modif : real    0m28,958s

user : uid=0(root) gid=0(root) groups=0(root),0(root),1(bin),2(daemon),3(sys),4(adm),6(disk),10(wheel),11(floppy),20(dialout),26(tape),27(video)

# après 3.8 :

 ## back-end :

tp-api:tp2      349MB      0B        

uid=1654(app) gid=1654(app) groups=1654(app)

après modif de program.cs : 

real    0m7,152s
user    0m0,098s
sys     0m0,032s

 ## front-end :

tp-api:tp2      63.4MB    0B        
uid=0(root) gid=0(root) groups=0(root),0(root),1(bin),2(daemon),3(sys),4(adm),6(disk),10(wheel),11(floppy),20(dialout),26(tape),27(video)
real    0m11,068s
user    0m0,091s
sys     0m0,046s

TP3

1) 3 problèmes (2 erreurs et 1 warning)

2) Aucune erreur concernait le bon fonctionnement du code

3) J'ai choisi la solution B, c'est la plus simple à mettre en place lors d'un tp, notamment lorsque je n'ai pas écrit tout le code du projet. 

4) ignore-unfixed : true n'est pas une bonne idée car elle laisse passer des potentielles failles graves

5) commentaires laissés : "le workflow fonctionne parfaitement et est visible, le seuil de couverture à été abaissé.
J'approuve la pull request"