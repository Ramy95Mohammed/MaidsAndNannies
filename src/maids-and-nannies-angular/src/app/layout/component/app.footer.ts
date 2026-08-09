import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
    standalone: true,
    selector: 'app-footer',
    imports: [RouterModule, TranslatePipe],
    template: `<div class="layout-footer">
        <span>{{ 'APP.NAME' | translate }} by
            <a href="https://primeng.org" target="_blank" rel="noopener noreferrer" class="text-primary font-bold hover:underline">prime-devv</a>
        </span>
        <a routerLink="/policies" class="text-primary font-medium hover:underline ml-3">{{ 'POLICIES.LINK' | translate }}</a>
    </div>`
})
export class AppFooter {}