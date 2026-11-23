====================================================================
CRIMSON CAMPUS - UNITY 2D (PIXEL ART PROCEDURAL)
====================================================================

CONTENU DE L'ARCHIVE :
- Assets/Scripts/CrimsonData.cs           : Donnees du campus, etudiantes, boutique et sauvegarde
- Assets/Scripts/PixelSpriteFactory.cs    : Generation procedurale de tous les sprites pixel art
- Assets/Scripts/RetroAudioManager.cs     : Synthetiseur audio 8-bit procedural
- Assets/Scripts/CrimsonCampusManager.cs  : Moteur principal, IA, dash, sacrifice, ennemis et UI

INSTALLATION RAPIDE DANS UNITY :
1. Creez un projet Unity 2D (Built-in ou URP, Unity 2021.3 / 2022.3 / Unity 6).
2. Verifiez dans Edit > Project Settings > Player > Other Settings > Active Input Handling
   que l'option est sur "Input Manager (Old)" ou "Both".
3. Copiez le dossier Assets/Scripts dans le dossier Assets de votre projet Unity.
4. Dans votre scene principale, creez un GameObject vide nomme "CrimsonCampusGame".
5. Attachez uniquement le script "CrimsonCampusManager" a ce GameObject.
6. Lancez le mode Play : tout le campus, les sprites, l'audio et l'interface sont generes automatiquement.

COMMANDES :
- ZQSD / WASD / Fleches / Clic gauche maintenu : Se deplacer
- SHIFT (gauche/droite) ou Clic droit          : Dash Chauve-Souris (invincibilite + vitesse x3.1 + nuee)
- ESPACE ou E                                  : Clin d'oeil (onde de charme de zone)
- F (ou rester sur l'Autel Cramoisi)           : Sacrifier toute la meute (+50% sur l'Autel)
- R (ecran de mort)                            : Recommencer la partie

NOUVEAUTES INCLUSES :
- Dash Chauve-Souris : transformation animee en chauve-souris, invincibilite pendant l'esquive,
  trainee de mini chauves-souris et recharge reduite avec l'amelioration "Ailes de Chauve-Souris".
- Chasseur de Vampire : apparait si trop de fideles te suivent (6+ par defaut).
- Loups-Garous : te traquent si tu accumules trop de sang non depense (300+ par defaut).
- Mort definitive : si un Chasseur ou un Loup-Garou t'attrape, tu meurs et recommences.
- Animation de sacrifice rituelle : sceau pentagramme rotatif, piliers de lumiere rouge,
  levitation en spirale, double onde de choc, screen shake et orbes de sang.
- Coeurs dans les yeux de toutes tes fideles charmees.
- Zero emoji dans le jeu et l'interface (icones pixel art generees en code).
