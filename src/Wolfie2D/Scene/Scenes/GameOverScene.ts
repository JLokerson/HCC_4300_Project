import Scene from "../Scene";
import Vec2 from "../../DataTypes/Vec2";
import Layer from "../Layer";
import Label from "../Nodes/UIElements/Label";
import Button from "../Nodes/UIElements/Button";
import Color from "../../Utils/Color";
import { GameEventType } from "../../Events/GameEventType";

export default class GameOverScene extends Scene {
    private gameOverLabel: Label;
    private restartButton: Button;
    private fadeLayer: Layer;

    public startScene(): void {
        // Create fade layer for dramatic effect
        this.fadeLayer = this.addUILayer("fade");
        
        // Create main UI layer
        const mainLayer = this.addUILayer("main");

        // Add dark overlay
        const overlay = this.add.graphic("overlay", "fade");
        overlay.color = Color.BLACK;
        overlay.alpha = 0.7;
        overlay.size = this.viewport.getHalfSize().scaled(4);
        overlay.position = this.viewport.getCenter();

        // Game Over title
        this.gameOverLabel = <Label>this.add.uiElement("label", "main", {
            position: new Vec2(this.viewport.getCenter().x, this.viewport.getCenter().y - 100),
            text: "GAME OVER"
        });
        this.gameOverLabel.textColor = Color.RED;
        this.gameOverLabel.fontSize = 48;
        this.gameOverLabel.alpha = 0;

        // Restart button
        this.restartButton = <Button>this.add.uiElement("button", "main", {
            position: new Vec2(this.viewport.getCenter().x, this.viewport.getCenter().y + 50),
            text: "Restart Game"
        });
        this.restartButton.backgroundColor = Color.GRAY;
        this.restartButton.borderColor = Color.WHITE;
        this.restartButton.textColor = Color.WHITE;
        this.restartButton.alpha = 0;

        // Add click handler
        this.restartButton.onClick = () => {
            this.emitter.fireEvent(GameEventType.RESTART_GAME);
        };

        // Start fade-in animation
        this.startFadeInAnimation();
    }

    private startFadeInAnimation(): void {
        // Fade in game over text
        this.gameOverLabel.tweens.add("fadeIn", {
            startDelay: 500,
            duration: 1000,
            effects: [{ property: "alpha", start: 0, end: 1 }]
        });

        // Fade in restart button
        this.restartButton.tweens.add("fadeIn", {
            startDelay: 1500,
            duration: 1000,
            effects: [{ property: "alpha", start: 0, end: 1 }]
        });
    }

    public updateScene(deltaT: number): void {
        // Game over scene doesn't need continuous updates
    }
}
