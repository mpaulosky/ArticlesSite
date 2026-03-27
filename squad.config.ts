import {
  defineSquad,
  defineTeam,
  defineAgent,
} from '@bradygaster/squad-sdk';

/**
 * Squad Configuration — ArticlesSite
 */
const scribe = defineAgent({
  name: 'scribe',
  role: 'scribe',
  description: 'Scribe',
  status: 'active',
});

const ralph = defineAgent({
  name: 'ralph',
  role: 'ralph',
  description: 'Ralph',
  status: 'active',
});

export default defineSquad({
  version: '1.0.0',

  team: defineTeam({
    name: 'ArticlesSite',
    members: ['scribe', 'ralph'],
  }),

  agents: [scribe, ralph],
});
